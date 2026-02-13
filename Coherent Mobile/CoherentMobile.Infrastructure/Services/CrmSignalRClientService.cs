using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Infrastructure.Services;

/// <summary>
/// Background service that maintains a SignalR connection to the CRM's CrmChatHub
/// to receive real-time doctor->patient messages and broadcast them to mobile clients
/// </summary>
public class CrmSignalRClientService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CrmSignalRClientService> _logger;
    private readonly string? _hubUrl;
    private readonly string? _apiKey;
    private HubConnection? _hubConnection;
    private readonly HashSet<string> _joinedThreads = new();
    private readonly object _threadLock = new();

    public CrmSignalRClientService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<CrmSignalRClientService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        // Get SignalR hub URL from configuration (prefer explicit SignalRHubUrl, fallback to constructing from V1BaseUrl)
        _hubUrl = configuration["CrmChatApi:SignalRHubUrl"];
        if (string.IsNullOrWhiteSpace(_hubUrl))
        {
            var baseUrl = configuration["CrmChatApi:V1BaseUrl"]?.TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                var uri = new Uri(baseUrl);
                _hubUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/hubs/crm-chat";
            }
        }
        _apiKey = configuration["CrmChatApi:ApiKey"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_hubUrl))
        {
            _logger.LogWarning("CRM SignalR hub URL not configured. Real-time chat sync disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndListenAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM SignalR connection error. Reconnecting in 5 seconds...");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task ConnectAndListenAsync(CancellationToken stoppingToken)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl!, options =>
            {
                if (!string.IsNullOrWhiteSpace(_apiKey))
                {
                    options.Headers.Add("X-API-Key", _apiKey);
                }
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();

        // Handle incoming messages from CRM (doctor -> patient)
        _hubConnection.On<CrmChatMessageEvent>("chat.message.created", async (msg) =>
        {
            await HandleIncomingMessageAsync(msg);
        });

        _hubConnection.On<CrmChatReadEvent>("chat.thread.read", async (evt) =>
        {
            _logger.LogInformation("Thread {ThreadId} marked as read by doctor {DoctorLicenseNo}", evt.CrmThreadId, evt.DoctorLicenseNo);
        });

        _hubConnection.Reconnecting += (error) =>
        {
            _logger.LogWarning(error, "CRM SignalR reconnecting...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += async (connectionId) =>
        {
            _logger.LogInformation("CRM SignalR reconnected with connectionId: {ConnectionId}", connectionId);
            await RejoinAllThreadsAsync();
        };

        _hubConnection.Closed += (error) =>
        {
            _logger.LogWarning(error, "CRM SignalR connection closed");
            return Task.CompletedTask;
        };

        await _hubConnection.StartAsync(stoppingToken);
        _logger.LogInformation("Connected to CRM SignalR hub at {HubUrl}", _hubUrl);

        // Join active conversation threads
        await JoinActiveThreadsAsync();

        // Keep alive
        while (!stoppingToken.IsCancellationRequested && _hubConnection.State == HubConnectionState.Connected)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            
            // Periodically check for new threads to join
            await JoinActiveThreadsAsync();
        }
    }

    private async Task HandleIncomingMessageAsync(CrmChatMessageEvent msg)
    {
        try
        {
            _logger.LogInformation(
                "Received CRM message: ThreadId={ThreadId}, MessageId={MessageId}, SenderType={SenderType}, SenderEmpId={SenderEmpId}, SenderEmpType={SenderEmpType}",
                msg.CrmThreadId, msg.CrmMessageId, msg.SenderType, msg.SenderEmpId, msg.SenderEmpType);

            // Process doctor -> patient OR staff -> patient messages
            var isDoctor = string.Equals(msg.SenderType, "Doctor", StringComparison.OrdinalIgnoreCase);
            var isStaff = string.Equals(msg.SenderType, "Staff", StringComparison.OrdinalIgnoreCase);
            
            if (!isDoctor && !isStaff)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            var patientRepo = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
            var pushSender = scope.ServiceProvider.GetRequiredService<IPushNotificationSender>();

            // Check for duplicate message using CrmMessageId
            if (!string.IsNullOrWhiteSpace(msg.CrmMessageId))
            {
                var exists = await chatRepo.CrmMessageExistsAsync(msg.CrmMessageId);
                if (exists)
                {
                    _logger.LogDebug("Skipping duplicate CRM message {MessageId}", msg.CrmMessageId);
                    return;
                }
            }

            // Find patient by MrNo
            if (string.IsNullOrWhiteSpace(msg.ReceiverMrNo))
            {
                _logger.LogWarning("ReceiverMrNo is empty for message {MessageId}", msg.CrmMessageId);
                return;
            }

            var patient = await patientRepo.GetByMRNOAsync(msg.ReceiverMrNo);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found for MrNo {MrNo}", msg.ReceiverMrNo);
                return;
            }

            // Parse conversation ID from CrmThreadId (format: CRM-TH-{conversationId})
            var conversationId = 0;
            if (!string.IsNullOrWhiteSpace(msg.CrmThreadId))
            {
                var threadIdStr = msg.CrmThreadId.StartsWith("CRM-TH-", StringComparison.OrdinalIgnoreCase)
                    ? msg.CrmThreadId.Substring("CRM-TH-".Length)
                    : msg.CrmThreadId;
                int.TryParse(threadIdStr, out conversationId);
            }

            if (conversationId <= 0)
            {
                _logger.LogWarning("Invalid CrmThreadId {ThreadId} for message {MessageId}", msg.CrmThreadId, msg.CrmMessageId);
                return;
            }

            // Get sender ID - for doctor use 0, for staff use EmpId
            var senderId = isStaff && msg.SenderEmpId.HasValue ? (int)msg.SenderEmpId.Value : 0;
            var senderType = isStaff ? "Staff" : "Doctor";

            // Save message to local database with CrmMessageId for deduplication
            var chatMessage = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                SenderType = senderType,
                MessageType = msg.MessageType ?? "Text",
                Content = msg.Content,
                FileUrl = msg.FileUrl,
                FileName = msg.FileName,
                FileSize = msg.FileSize,
                SentAt = msg.SentAt == default ? DateTime.UtcNow : msg.SentAt,
                CrmMessageId = msg.CrmMessageId
            };

            var messageId = await chatRepo.SaveCrmMessageAsync(chatMessage, msg.CrmMessageId ?? Guid.NewGuid().ToString());
            chatMessage.MessageId = messageId;

            // Update conversation last message
            var lastMessage = chatMessage.MessageType == "Text"
                ? chatMessage.Content
                : $"[{chatMessage.MessageType}]";
            await chatRepo.UpdateConversationLastMessageAsync(conversationId, lastMessage ?? "", DateTime.UtcNow);

            _logger.LogInformation(
                "Saved CRM message {CrmMessageId} as local message {MessageId} in conversation {ConversationId}",
                msg.CrmMessageId, messageId, conversationId);

            // Determine notification title based on sender type
            var staffTypeName = GetStaffTypeName(msg.SenderEmpType);
            var title = isStaff 
                ? $"New Message from {staffTypeName}" 
                : "New Message from Doctor";

            // Send push notification to patient
            var notificationData = new Dictionary<string, string>
            {
                ["type"] = "chat_message",
                ["conversationId"] = conversationId.ToString(),
                ["messageId"] = messageId.ToString(),
                ["crmThreadId"] = msg.CrmThreadId ?? "",
                ["crmMessageId"] = msg.CrmMessageId ?? "",
                ["senderType"] = senderType,
                ["messageType"] = msg.MessageType ?? "Text"
            };

            if (isStaff)
            {
                notificationData["senderEmpId"] = msg.SenderEmpId?.ToString() ?? "";
                notificationData["senderEmpType"] = msg.SenderEmpType?.ToString() ?? "";
            }

            var body = msg.MessageType == "Text" 
                ? (msg.Content?.Length > 100 ? msg.Content.Substring(0, 100) + "..." : msg.Content ?? "You have a new message")
                : $"Sent a {msg.MessageType?.ToLower() ?? "message"}";

            await pushSender.SendWakeupAsync(
                patient.Id,
                "Patient",
                title,
                body,
                notificationData);

            _logger.LogInformation(
                "Push notification sent to patient {PatientId} for message {MessageId}",
                patient.Id, msg.CrmMessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling incoming CRM message {MessageId}", msg.CrmMessageId);
        }
    }

    private async Task JoinActiveThreadsAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

            // Get active conversations (last 7 days)
            var activeThreadIds = await chatRepo.GetActiveThreadIdsAsync(TimeSpan.FromDays(7));

            foreach (var threadId in activeThreadIds)
            {
                var crmThreadId = $"CRM-TH-{threadId}";
                
                lock (_threadLock)
                {
                    if (_joinedThreads.Contains(crmThreadId))
                        continue;
                }

                try
                {
                    await _hubConnection.InvokeAsync("JoinThread", crmThreadId);
                    
                    lock (_threadLock)
                    {
                        _joinedThreads.Add(crmThreadId);
                    }
                    
                    _logger.LogDebug("Joined CRM thread {ThreadId}", crmThreadId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to join thread {ThreadId}", crmThreadId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining active threads");
        }
    }

    private async Task RejoinAllThreadsAsync()
    {
        List<string> threadsToRejoin;
        lock (_threadLock)
        {
            threadsToRejoin = _joinedThreads.ToList();
            _joinedThreads.Clear();
        }

        foreach (var threadId in threadsToRejoin)
        {
            try
            {
                await _hubConnection!.InvokeAsync("JoinThread", threadId);
                lock (_threadLock)
                {
                    _joinedThreads.Add(threadId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to rejoin thread {ThreadId}", threadId);
            }
        }
    }

    public async Task JoinThreadAsync(string crmThreadId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
            return;

        lock (_threadLock)
        {
            if (_joinedThreads.Contains(crmThreadId))
                return;
        }

        try
        {
            await _hubConnection.InvokeAsync("JoinThread", crmThreadId);
            lock (_threadLock)
            {
                _joinedThreads.Add(crmThreadId);
            }
            _logger.LogDebug("Joined CRM thread {ThreadId}", crmThreadId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to join thread {ThreadId}", crmThreadId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }

    private static string GetStaffTypeName(int? empType)
    {
        return empType switch
        {
            1 => "Doctor",
            2 => "Nurse",
            3 => "Receptionist",
            4 => "IVF Lab",
            5 => "Admin",
            _ => "Staff"
        };
    }
}

// DTOs for CRM SignalR events
public class CrmChatMessageEvent
{
    public string? CrmThreadId { get; set; }
    public string? CrmMessageId { get; set; }
    public string? SenderType { get; set; }
    public string? SenderMrNo { get; set; }
    public string? SenderDoctorLicenseNo { get; set; }
    public long? SenderEmpId { get; set; }
    public int? SenderEmpType { get; set; }
    public string? ReceiverType { get; set; }
    public string? ReceiverMrNo { get; set; }
    public string? ReceiverDoctorLicenseNo { get; set; }
    public string? ReceiverStaffType { get; set; }
    public string? MessageType { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public DateTime SentAt { get; set; }
}

public class CrmChatReadEvent
{
    public string? CrmThreadId { get; set; }
    public string? DoctorLicenseNo { get; set; }
    public DateTime ReadAtUtc { get; set; }
}
