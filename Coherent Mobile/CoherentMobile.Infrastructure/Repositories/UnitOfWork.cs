using System.Data;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DapperContext _context;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public IUserRepository Users { get; }
    public IPatientRepository Patients { get; }
    public IHealthRecordRepository HealthRecords { get; }

    public UnitOfWork(DapperContext context, IUserRepository userRepository, IPatientRepository patientRepository, IHealthRecordRepository healthRecordRepository)
    {
        _context = context;
        Users = userRepository;
        Patients = patientRepository;
        HealthRecords = healthRecordRepository;
    }

    public async Task<int> CommitAsync()
    {
        // For Dapper, this is handled at repository level
        return await Task.FromResult(1);
    }

    public async Task BeginTransactionAsync()
    {
        _connection = _context.CreateConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();
        await Task.CompletedTask;
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        await Task.CompletedTask;
    }

    public async Task RollbackTransactionAsync()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}
