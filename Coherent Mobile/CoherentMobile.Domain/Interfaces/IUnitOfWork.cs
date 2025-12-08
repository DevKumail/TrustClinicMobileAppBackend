namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPatientRepository Patients { get; }
    IHealthRecordRepository HealthRecords { get; }
    Task<int> CommitAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
