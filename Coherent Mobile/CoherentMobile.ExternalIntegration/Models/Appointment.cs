using System;

namespace CoherentMobile.ExternalIntegration.Models
{
    public class Appointment
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? AppointmentDateTime { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int AppId { get; set; }
        public string MrNo { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Speciality { get; set; }
    }
}
