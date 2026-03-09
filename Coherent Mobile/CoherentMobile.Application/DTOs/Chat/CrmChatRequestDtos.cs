using System;
using System.Text.Json.Serialization;

namespace CoherentMobile.Application.DTOs.Chat
{
    public sealed class CrmGetOrCreateThreadRequest
    {
        [JsonPropertyName("patientMrNo")]
        public string PatientMrNo { get; set; } = string.Empty;

        [JsonPropertyName("doctorLicenseNo")]
        public string DoctorLicenseNo { get; set; } = string.Empty;

        [JsonPropertyName("sourceSystem")]
        public string SourceSystem { get; set; } = string.Empty;
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

        [JsonPropertyName("senderEmpId")]
        public long? SenderEmpId { get; set; }

        [JsonPropertyName("senderEmpType")]
        public int? SenderEmpType { get; set; }

        [JsonPropertyName("receiverType")]
        public string ReceiverType { get; set; } = string.Empty;

        [JsonPropertyName("receiverMrNo")]
        public string? ReceiverMrNo { get; set; }

        [JsonPropertyName("receiverDoctorLicenseNo")]
        public string? ReceiverDoctorLicenseNo { get; set; }

        [JsonPropertyName("receiverStaffType")]
        public string? ReceiverStaffType { get; set; }

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

    public sealed class CrmGetOrCreateBroadcastChannelRequest
    {
        [JsonPropertyName("patientMrNo")]
        public string PatientMrNo { get; set; } = string.Empty;

        [JsonPropertyName("staffType")]
        public string StaffType { get; set; } = string.Empty;
    }
}
