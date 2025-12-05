# 🗄️ Coherent Mobile - Repository Setup Guide

## ✅ **What Has Been Done:**

### 1. **Database Connection** 
✅ Updated `appsettings.json` with production database connection string:
```
Server: 175.107.195.221
Database: CoherentMobApp
User: Tekno
```

### 2. **Generic Repository Pattern**
✅ Created `GenericRepository<T>` class with INT IDENTITY support:
- CRUD operations (Add, Update, Delete, GetById, GetAll)
- Stored procedure execution methods
- Works with `int` primary keys (not GUID)

### 3. **Repository Interfaces Created**
📁 `CoherentMobile.Domain/Interfaces/`

✅ **IPatientRepository** - User/Patient operations
- GetByMRNO, GetByEmiratesId, GetByPassportNumber
- GetByIdentity (uses SP)
- UpdateFailedLoginAttempts, ResetFailedLoginAttempts
- Soft delete support

✅ **IOTPVerificationRepository** - OTP management
- GetLatestByMRNO, GetLatestByPatientId
- VerifyOTP (uses SP: `sp_VerifyOTP`)
- MarkAsExpired, IncrementAttemptCount

✅ **IQRCodeScanRepository** - QR scan tracking
- GetByQRCodeId, GetByMRNO
- MarkAsCompleted
- GetPendingScans

✅ **IPasswordResetTokenRepository** - Password reset
- GetByToken, GetLatestByPatientId
- MarkAsUsed, MarkAsExpired
- GetExpiredTokens

✅ **IAuthAuditLogRepository** - Audit logging
- GetByPatientId, GetByMRNO, GetByAction
- GetRecentLogs
- AddAsync for logging

---

### 4. **Repository Implementations Created**
📁 `CoherentMobile.Infrastructure/Repositories/`

All repository classes implement their interfaces and extend `GenericRepository<T>`:

| Repository | Table | Key Features |
|------------|-------|--------------|
| `PatientRepository` | Users | Identity lookup, failed login tracking |
| `OTPVerificationRepository` | OTPVerifications | OTP verification, expiry management |
| `QRCodeScanRepository` | QRCodeScans | QR scan tracking, completion status |
| `PasswordResetTokenRepository` | PasswordResetTokens | Token validation, expiry |
| `AuthAuditLogRepository` | AuthAuditLogs | Action logging, audit trail |

---

### 5. **Dependency Injection**
✅ All repositories registered in `DependencyInjection.cs`:

```csharp
services.AddScoped<IPatientRepository, PatientRepository>();
services.AddScoped<IOTPVerificationRepository, OTPVerificationRepository>();
services.AddScoped<IQRCodeScanRepository, QRCodeScanRepository>();
services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
services.AddScoped<IAuthAuditLogRepository, AuthAuditLogRepository>();
```

---

## 📊 **Database Tables Mapped:**

### **Users Table** (Patient Entity)
- Primary Key: `Id INT IDENTITY(1,1)`
- Unique: MRNO, EmiratesId, PassportNumber
- Features: Soft delete, failed login tracking

### **OTPVerifications Table**
- Foreign Key: `PatientId` → Users(Id)
- OTP Types: Signup, Login, ForgotPassword
- Channels: SMS, Email
- Auto-expiry: 5 minutes

### **QRCodeScans Table**
- Tracks QR code scans
- Links to MRNO
- Signup completion status

### **PasswordResetTokens Table**
- Foreign Key: `PatientId` → Users(Id)
- Token expiry: 30 minutes
- One-time use

### **AuthAuditLogs Table**
- Complete authentication audit trail
- Actions: QRScan, OTPSent, Login, Logout, etc.
- Status: Success, Failed, Pending

---

## 🔧 **Stored Procedures Used:**

| Procedure | Description |
|-----------|-------------|
| `sp_GetPatientByIdentity` | Get user by Emirates ID or Passport |
| `sp_VerifyOTP` | Verify OTP and update attempt count |
| `sp_UpdateFailedLoginAttempts` | Increment failed login count |
| `sp_ResetFailedLoginAttempts` | Reset on successful login |

---

## 📝 **How to Use Repositories:**

### **Example 1: Get Patient by MRNO**
```csharp
public class AuthService
{
    private readonly IPatientRepository _patientRepo;
    
    public AuthService(IPatientRepository patientRepo)
    {
        _patientRepo = patientRepo;
    }
    
    public async Task<Patient?> GetPatientAsync(string mrno)
    {
        return await _patientRepo.GetByMRNOAsync(mrno);
    }
}
```

### **Example 2: Verify OTP**
```csharp
public class OTPService
{
    private readonly IOTPVerificationRepository _otpRepo;
    
    public async Task<bool> VerifyAsync(string otpCode, string mrno)
    {
        return await _otpRepo.VerifyOTPAsync(otpCode, mrno, null);
    }
}
```

### **Example 3: Add Audit Log**
```csharp
public class AuditService
{
    private readonly IAuthAuditLogRepository _auditRepo;
    
    public async Task LogActionAsync(int patientId, string action, string status)
    {
        var log = new AuthAuditLog
        {
            PatientId = patientId,
            Action = action,
            Status = status,
            IPAddress = GetClientIP(),
            CreatedAt = DateTime.UtcNow
        };
        
        await _auditRepo.AddAsync(log);
    }
}
```

### **Example 4: Track QR Scan**
```csharp
public class QRService
{
    private readonly IQRCodeScanRepository _qrRepo;
    
    public async Task<int> RecordScanAsync(string qrCodeId, string mrno)
    {
        var scan = new QRCodeScan
        {
            QRCodeId = qrCodeId,
            MRNO = mrno,
            ScannedAt = DateTime.UtcNow,
            IPAddress = GetClientIP()
        };
        
        return await _qrRepo.AddAsync(scan);
    }
}
```

---

## 🎯 **Generic Repository Methods:**

All repositories inherit these methods from `GenericRepository<T>`:

| Method | Description |
|--------|-------------|
| `GetByIdAsync(int id)` | Get entity by ID |
| `GetAllAsync()` | Get all entities |
| `AddAsync(T entity)` | Insert and return new ID |
| `UpdateAsync(T entity)` | Update entity |
| `DeleteAsync(int id)` | Hard delete |
| `ExistsAsync(int id)` | Check if exists |
| `ExecuteStoredProcSingleAsync` | Execute SP, return single |
| `ExecuteStoredProcAsync` | Execute SP, return multiple |
| `ExecuteStoredProcNonQueryAsync` | Execute SP, no return |

---

## ⚠️ **Important Notes:**

1. **INT vs GUID**: All IDs are now `int` (not `Guid`)
2. **No Session Table**: JWT tokens handle sessions (no LoginSessions table repository needed)
3. **Soft Delete**: Patient/User table supports soft delete (`IsDeleted` flag)
4. **Connection String**: Using production database (not localhost)
5. **Stored Procedures**: Some operations use SQL stored procedures for complex logic

---

## 🚀 **Next Steps:**

1. ✅ Build the solution to verify no errors
2. Create DTOs for API requests/responses
3. Create Application layer services:
   - `AuthenticationService`
   - `OTPService`
   - `QRCodeService`
   - `PasswordResetService`
4. Create API controllers:
   - `AuthController`
   - `QRScanController`
5. Add FluentValidation validators
6. Test database connectivity
7. Implement complete signup flow

---

## 📚 **File Structure:**

```
CoherentMobile.Domain/
├── Entities/
│   ├── Patient.cs
│   ├── OTPVerification.cs
│   ├── QRCodeScan.cs
│   ├── PasswordResetToken.cs
│   └── AuthAuditLog.cs
└── Interfaces/
    ├── IPatientRepository.cs
    ├── IOTPVerificationRepository.cs
    ├── IQRCodeScanRepository.cs
    ├── IPasswordResetTokenRepository.cs
    └── IAuthAuditLogRepository.cs

CoherentMobile.Infrastructure/
├── Data/
│   └── DapperContext.cs
├── Repositories/
│   ├── GenericRepository.cs
│   ├── PatientRepository.cs
│   ├── OTPVerificationRepository.cs
│   ├── QRCodeScanRepository.cs
│   ├── PasswordResetTokenRepository.cs
│   └── AuthAuditLogRepository.cs
└── DependencyInjection.cs

CoherentMobile.Api/
├── appsettings.json (Updated connection string)
└── Database/
    └── CreateUserAuthTables.sql
```

---

**Setup Complete! All repositories are ready to use.** ✨

**Database:** CoherentMobApp (Production)  
**Primary Key Type:** INT IDENTITY(1,1)  
**ORM:** Dapper (High Performance)
