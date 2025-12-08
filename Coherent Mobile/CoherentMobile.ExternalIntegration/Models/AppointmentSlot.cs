using CoherentMobile.ExternalIntegration.Converters;
using System.Text.Json.Serialization;

namespace CoherentMobile.ExternalIntegration.Models
{
    public class AvailableSlot
    {
        [JsonPropertyName("slotId")]
        public string SlotId { get; set; } = string.Empty;

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [JsonPropertyName("dttmFrom")]
        public DateTime DttmFrom { get; set; }

        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        [JsonPropertyName("dttmTo")]
        public DateTime DttmTo { get; set; }
        
        [JsonPropertyName("dttmDuration")]
        public string DttmDuration { get; set; } = string.Empty;
        
        [JsonPropertyName("slotState")]
        public string SlotState { get; set; } = string.Empty;
        
        [JsonPropertyName("slotType")]
        public string SlotType { get; set; } = string.Empty;

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [JsonPropertyName("updtDttm")]
        public DateTime UpdtDttm { get; set; }
    }

    public class DoctorSlotResponse
    {
        [JsonPropertyName("specialityId")]
        public string SpecialityId { get; set; } = string.Empty;
        
        [JsonPropertyName("specialityName")]
        public string? SpecialityName { get; set; }
        
        [JsonPropertyName("facilityId")]
        public string FacilityId { get; set; } = string.Empty;
        
        [JsonPropertyName("resourceCd")]
        public string ResourceCd { get; set; } = string.Empty;
        
        [JsonPropertyName("prsnlId")]
        public string PrsnlId { get; set; } = string.Empty;
        
        [JsonPropertyName("prsnlName")]
        public string PrsnlName { get; set; } = string.Empty;
        
        [JsonPropertyName("resourceName")]
        public string ResourceName { get; set; } = string.Empty;
        
        [JsonPropertyName("prsnlAlias")]
        public string PrsnlAlias { get; set; } = string.Empty;

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [JsonPropertyName("execDttmFrom")]
        public DateTime ExecDttmFrom { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [JsonPropertyName("execDttmTo")]
        public DateTime ExecDttmTo { get; set; }
        
        [JsonPropertyName("availableSlots")]
        public List<AvailableSlot> AvailableSlots { get; set; } = new();
    }

    public class DoctorSlotsApiResponse
    {
        [JsonPropertyName("data")]
        public List<DoctorSlotResponse> Data { get; set; } = new();
    }
}
