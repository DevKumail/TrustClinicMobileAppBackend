using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoherentMobile.ExternalIntegration.Models
{
    public sealed class StringOrNumberToStringJsonConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var l) ? l.ToString() : reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing string")
            };
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value);
        }
    }

    public sealed class CrmGetOrCreateThreadRequest
    {
        [JsonPropertyName("patientMrNo")]
        public string PatientMrNo { get; set; } = string.Empty;

        [JsonPropertyName("doctorLicenseNo")]
        public string DoctorLicenseNo { get; set; } = string.Empty;

        [JsonPropertyName("sourceSystem")]
        public string SourceSystem { get; set; } = string.Empty;
    }

    public sealed class CrmGetOrCreateThreadResponse
    {
        [JsonPropertyName("crmThreadId")]
        public string CrmThreadId { get; set; } = string.Empty;

        [JsonPropertyName("patientMrNo")]
        public string PatientMrNo { get; set; } = string.Empty;

        [JsonPropertyName("doctorLicenseNo")]
        public string DoctorLicenseNo { get; set; } = string.Empty;
    }

    public sealed class CrmSendMessageRequest
    {
        [JsonPropertyName("crmThreadId")]
        public string CrmThreadId { get; set; } = string.Empty;

        [JsonPropertyName("senderType")]
        public string SenderType { get; set; } = string.Empty;

        [JsonPropertyName("senderMrNo")]
        public string? SenderMrNo { get; set; }

        [JsonPropertyName("senderDoctorLicenseNo")]
        public string? SenderDoctorLicenseNo { get; set; }

        [JsonPropertyName("receiverType")]
        public string ReceiverType { get; set; } = string.Empty;

        [JsonPropertyName("receiverMrNo")]
        public string? ReceiverMrNo { get; set; }

        [JsonPropertyName("receiverDoctorLicenseNo")]
        public string? ReceiverDoctorLicenseNo { get; set; }

        [JsonPropertyName("messageType")]
        public string MessageType { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("fileUrl")]
        public string? FileUrl { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("fileSize")]
        public long? FileSize { get; set; }

        [JsonPropertyName("clientMessageId")]
        public string? ClientMessageId { get; set; }

        [JsonPropertyName("sentAt")]
        public DateTime? SentAt { get; set; }
    }

    public sealed class CrmSendMessageResponse
    {
        [JsonPropertyName("crmMessageId")]
        public string CrmMessageId { get; set; } = string.Empty;

        [JsonPropertyName("crmThreadId")]
        public string CrmThreadId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("serverReceivedAt")]
        public DateTime? ServerReceivedAt { get; set; }
    }

    public sealed class CrmMessageUpdateEvent
    {
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("crmThreadId")]
        public string CrmThreadId { get; set; } = string.Empty;

        [JsonPropertyName("crmMessageId")]
        public string CrmMessageId { get; set; } = string.Empty;

        [JsonPropertyName("doctorLicenseNo")]
        public string? DoctorLicenseNo { get; set; }

        [JsonPropertyName("patientMrNo")]
        public string? PatientMrNo { get; set; }

        [JsonPropertyName("messageType")]
        public string? MessageType { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("fileUrl")]
        public string? FileUrl { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("fileSize")]
        public long? FileSize { get; set; }

        [JsonPropertyName("sentAt")]
        public DateTime? SentAt { get; set; }
    }

    public sealed class CrmConversationCounterpart
    {
        [JsonPropertyName("userType")]
        public string UserType { get; set; } = string.Empty;

        [JsonPropertyName("patientMrNo")]
        public string? PatientMrNo { get; set; }

        [JsonPropertyName("patientName")]
        public string? PatientName { get; set; }

        [JsonPropertyName("doctorLicenseNo")]
        public string? DoctorLicenseNo { get; set; }

        [JsonPropertyName("doctorName")]
        public string? DoctorName { get; set; }

        [JsonPropertyName("doctorTitle")]
        public string? DoctorTitle { get; set; }

        [JsonPropertyName("doctorPhotoName")]
        public string? DoctorPhotoName { get; set; }
    }

    public sealed class CrmConversationListItem
    {
        [JsonPropertyName("conversationId")]
        [JsonConverter(typeof(StringOrNumberToStringJsonConverter))]
        public string? ConversationId { get; set; }

        [JsonPropertyName("crmThreadId")]
        public string CrmThreadId { get; set; } = string.Empty;

        [JsonPropertyName("lastMessageAt")]
        public DateTime? LastMessageAt { get; set; }

        [JsonPropertyName("lastMessagePreview")]
        public string? LastMessagePreview { get; set; }

        [JsonPropertyName("unreadCount")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("counterpart")]
        public CrmConversationCounterpart Counterpart { get; set; } = new();
    }

    public sealed class CrmConversationListResponse
    {
        [JsonPropertyName("doctorLicenseNo")]
        public string? DoctorLicenseNo { get; set; }

        [JsonPropertyName("patientMrNo")]
        public string? PatientMrNo { get; set; }

        [JsonPropertyName("serverTimeUtc")]
        public DateTime? ServerTimeUtc { get; set; }

        [JsonPropertyName("conversations")]
        public List<CrmConversationListItem> Conversations { get; set; } = new();
    }
}
