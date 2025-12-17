using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoherentMobile.ExternalIntegration.Models;

namespace CoherentMobile.ExternalIntegration.Interfaces
{
    public interface IAppointmentApiClient
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByMrnoAsync(string mrno);
        Task<DoctorSlotsApiResponse> GetAvailableDoctorSlotsAsync( string prsnlAlias, DateTime fromDate, DateTime toDate);
        Task<BookAppointmentResponse> BookAppointmentAsync(BookAppointmentRequest request);
        Task<CancelAppointmentResponse> CancelAppointmentAsync(CancelAppointmentRequest request);
        Task<ChangeBookedAppointmentResponse> ChangeBookedAppointmentAsync(ChangeBookedAppointmentRequest request);
    }
}
