using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoherentMobile.ExternalIntegration.Models;

namespace CoherentMobile.ExternalIntegration.Interfaces
{
    public interface IAppointmentApiClient
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByMrnoAsync(string mrno);
        Task<DoctorSlotsApiResponse> GetAvailableDoctorSlotsAsync(int doctorId, string prsnlAlias, DateTime fromDate, DateTime toDate);
    }
}
