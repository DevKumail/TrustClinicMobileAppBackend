using System.Collections.Generic;
using System.Threading.Tasks;
using CoherentMobile.ExternalIntegration.Models;

namespace CoherentMobile.ExternalIntegration.Interfaces
{
    public interface IPatientHealthApiClient
    {
        Task<IEnumerable<Medication>> GetMedicationsByMrnoV2Async(string mrno);
        Task<IEnumerable<Allergy>> GetAllergiesByMrnoAsync(string mrno);
        Task<IEnumerable<Diagnosis>> GetDiagnosesByMrnoAsync(string mrno);
    }
}
