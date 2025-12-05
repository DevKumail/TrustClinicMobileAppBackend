# 🛡️ Coherent Mobile - Exception Handling Guide

## ✅ **Complete Exception Handling Implemented!**

---

## 📋 **What Was Added:**

### **1. Custom Exception Classes** ✅
📁 `CoherentMobile.Application/Exceptions/`

#### **AuthenticationException**
- **Purpose:** Authentication and authorization failures
- **HTTP Status:** 401 Unauthorized
- **Error Codes:**
  - `AUTH_ERROR` - Generic authentication error
  - `VERIFY_INFO_ERROR` - Information verification failed
  - `CREATE_PROFILE_ERROR` - Profile creation failed
  - `LOGIN_ERROR` - Login failed
  - `FORGOT_PASSWORD_ERROR` - Forgot password failed
  - `RESET_PASSWORD_ERROR` - Password reset failed

#### **OTPException**
- **Purpose:** OTP-related failures
- **HTTP Status:** 400 Bad Request
- **Error Codes:**
  - `OTP_ERROR` - Generic OTP error
  - `VERIFY_OTP_ERROR` - OTP verification failed

#### **ValidationException**
- **Purpose:** Input validation failures
- **HTTP Status:** 400 Bad Request
- **Error Code:** `VALIDATION_ERROR`
- **Contains:** Dictionary of field-level errors

#### **DatabaseException**
- **Purpose:** Database operation failures
- **HTTP Status:** 500 Internal Server Error
- **Error Codes:**
  - `DB_ERROR` - Generic database error
  - `DB_VERIFY_INFO_ERROR` - Database error during info verification
  - `DB_VERIFY_OTP_ERROR` - Database error during OTP verification
  - `DB_CREATE_PROFILE_ERROR` - Database error during profile creation
  - `DB_LOGIN_ERROR` - Database error during login
  - `DB_FORGOT_PASSWORD_ERROR` - Database error during forgot password
  - `DB_RESET_PASSWORD_ERROR` - Database error during password reset
  - `DB_GET_BY_ID_ERROR` - Failed to get entity by ID
  - `DB_GET_ALL_ERROR` - Failed to get all entities
  - `DB_ADD_ERROR` - Failed to add entity
  - `DB_UPDATE_ERROR` - Failed to update entity
  - `DB_DELETE_ERROR` - Failed to delete entity
  - `DB_EXISTS_ERROR` - Failed to check existence
  - `DB_SP_ERROR` - Stored procedure execution error

---

## 🔧 **2. Global Exception Handler Middleware** ✅
📄 `CoherentMobile.Api/Middleware/ErrorHandlingMiddleware.cs`

**Catches all unhandled exceptions and returns structured error responses:**

```csharp
{
  "success": false,
  "message": "Error message",
  "errorCode": "ERROR_CODE",
  "details": "Additional details",
  "errors": { /* field errors */ },
  "timestamp": "2024-12-01T12:00:00Z"
}
```

**Exception Mapping:**

| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| `ValidationException` | 400 Bad Request | VALIDATION_ERROR |
| `AuthenticationException` | 401 Unauthorized | Custom Code |
| `OTPException` | 400 Bad Request | Custom Code |
| `DatabaseException` | 500 Internal Server Error | Custom Code |
| `ArgumentException` | 400 Bad Request | INVALID_ARGUMENT |
| `InvalidOperationException` | 400 Bad Request | INVALID_OPERATION |
| `KeyNotFoundException` | 404 Not Found | NOT_FOUND |
| `UnauthorizedAccessException` | 401 Unauthorized | UNAUTHORIZED |
| **Default** | 500 Internal Server Error | INTERNAL_SERVER_ERROR |

---

## 🔄 **3. Service Layer Exception Handling** ✅
📄 `CoherentMobile.Application/Services/AuthenticationService.cs`

**All methods wrapped with try-catch blocks:**

```csharp
public async Task<VerifyInformationResponseDto> VerifyInformationAsync(...)
{
    try
    {
        // Business logic
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Database error...");
        await LogAuditAsync(...);
        throw new DatabaseException("...", ex, "DB_VERIFY_INFO_ERROR");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error...");
        await LogAuditAsync(...);
        throw new AuthenticationException("...", ex, "VERIFY_INFO_ERROR");
    }
}
```

**All service methods include:**
- ✅ Try-catch for database errors
- ✅ Try-catch for unexpected errors
- ✅ Logging with context
- ✅ Audit trail logging
- ✅ Custom exception throwing

---

## 💾 **4. Repository Layer Exception Handling** ✅
📄 `CoherentMobile.Infrastructure/Repositories/GenericRepository.cs`

**All database operations wrapped:**

```csharp
public virtual async Task<T?> GetByIdAsync(int id)
{
    try
    {
        // Database query
    }
    catch (SqlException ex)
    {
        throw new DatabaseException($"Failed to get {typeof(T).Name} by ID: {id}", ex, "DB_GET_BY_ID_ERROR");
    }
}
```

**Protected methods:**
- ✅ `GetByIdAsync`
- ✅ `GetAllAsync`
- ✅ `AddAsync`
- ✅ `UpdateAsync`
- ✅ `DeleteAsync`
- ✅ `ExistsAsync`
- ✅ `ExecuteStoredProcSingleAsync`
- ✅ `ExecuteStoredProcAsync`
- ✅ `ExecuteStoredProcNonQueryAsync`

---

## 📊 **Error Response Format:**

### **Success Response:**
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... }
}
```

### **Validation Error (400):**
```json
{
  "success": false,
  "message": "Validation failed",
  "errorCode": "VALIDATION_ERROR",
  "errors": {
    "emiratesId": ["Emirates ID must be in format XXX-XXXX-XXXXXXX-X"],
    "password": ["Password must be at least 8 characters"]
  },
  "timestamp": "2024-12-01T12:00:00Z"
}
```

### **Authentication Error (401):**
```json
{
  "success": false,
  "message": "Invalid credentials",
  "errorCode": "LOGIN_ERROR",
  "timestamp": "2024-12-01T12:00:00Z"
}
```

### **Database Error (500):**
```json
{
  "success": false,
  "message": "A database error occurred. Please try again later.",
  "errorCode": "DB_LOGIN_ERROR",
  "details": "Contact support if the problem persists",
  "timestamp": "2024-12-01T12:00:00Z"
}
```

### **Generic Error (500):**
```json
{
  "success": false,
  "message": "An unexpected error occurred. Please try again later.",
  "errorCode": "INTERNAL_SERVER_ERROR",
  "details": "Contact support if the problem persists",
  "timestamp": "2024-12-01T12:00:00Z"
}
```

---

## 🎯 **Error Flow:**

```
1. Exception occurs in Repository/Service
   ↓
2. Caught by method try-catch
   ↓
3. Logged with context
   ↓
4. Custom exception thrown
   ↓
5. Caught by GlobalExceptionHandler
   ↓
6. Mapped to HTTP status code
   ↓
7. Structured error response returned
   ↓
8. Client receives standardized error
```

---

## 📝 **Logging:**

**All errors are logged with:**
- ✅ Exception details
- ✅ Stack trace
- ✅ Context (MRNO, PatientId, etc.)
- ✅ Timestamp
- ✅ Audit trail (for authentication operations)

**Example log entry:**
```
[ERR] 2024-12-01 12:00:00.123 
Database error in VerifyInformationAsync for MRNO: MRN001234
System.Data.SqlClient.SqlException: Timeout expired
```

---

## 🔍 **Error Codes Reference:**

### **Authentication Errors (401):**
- `AUTH_ERROR` - Generic authentication failure
- `UNAUTHORIZED` - Unauthorized access
- `LOGIN_ERROR` - Login failed

### **Validation Errors (400):**
- `VALIDATION_ERROR` - Input validation failed
- `INVALID_ARGUMENT` - Invalid argument provided
- `INVALID_OPERATION` - Invalid operation attempted
- `VERIFY_INFO_ERROR` - Information verification failed
- `VERIFY_OTP_ERROR` - OTP verification failed

### **OTP Errors (400):**
- `OTP_ERROR` - Generic OTP error
- `VERIFY_OTP_ERROR` - OTP verification failed

### **Database Errors (500):**
- `DB_ERROR` - Generic database error
- `DB_VERIFY_INFO_ERROR` - Database error during verification
- `DB_VERIFY_OTP_ERROR` - Database error during OTP check
- `DB_CREATE_PROFILE_ERROR` - Database error during profile creation
- `DB_LOGIN_ERROR` - Database error during login
- `DB_FORGOT_PASSWORD_ERROR` - Database error during forgot password
- `DB_RESET_PASSWORD_ERROR` - Database error during password reset
- `DB_GET_BY_ID_ERROR` - Failed to retrieve by ID
- `DB_GET_ALL_ERROR` - Failed to retrieve all
- `DB_ADD_ERROR` - Failed to insert
- `DB_UPDATE_ERROR` - Failed to update
- `DB_DELETE_ERROR` - Failed to delete
- `DB_EXISTS_ERROR` - Failed to check existence
- `DB_SP_ERROR` - Stored procedure failed

### **Other Errors:**
- `NOT_FOUND` - Resource not found (404)
- `INTERNAL_SERVER_ERROR` - Unexpected error (500)

---

## 🧪 **Testing Error Handling:**

### **1. Test Validation Error:**
```bash
POST /api/authentication/verify-information
{
  "mrno": "MRN001",
  "emiratesId": "invalid",  # Wrong format
  "mobileNumber": "123"     # Wrong format
}

Expected: 400 Bad Request with VALIDATION_ERROR
```

### **2. Test Authentication Error:**
```bash
POST /api/authentication/login
{
  "identifier": "784-1990-1234567-1",
  "password": "wrongpassword"
}

Expected: 401 Unauthorized with LOGIN_ERROR
```

### **3. Test Database Error:**
```bash
# Simulate by stopping database connection
Expected: 500 Internal Server Error with DB_*_ERROR
```

---

## ✅ **Best Practices Implemented:**

1. ✅ **Separation of Concerns**
   - Custom exceptions in Application layer
   - Repository throws database exceptions
   - Service layer catches and rethrows with context
   - Controller relies on global handler

2. ✅ **Logging**
   - All errors logged with structured logging
   - Context preserved (MRNO, IDs, etc.)
   - Sensitive data masked in logs

3. ✅ **Security**
   - Database errors don't expose schema
   - Generic messages for production
   - Detailed logs for debugging

4. ✅ **User Experience**
   - Clear, actionable error messages
   - Consistent error structure
   - Error codes for client handling

5. ✅ **Audit Trail**
   - Failed auth attempts logged
   - Database errors tracked
   - Security events audited

---

## 🎉 **Summary:**

**Total Exception Handlers:** 30+  
**Custom Exception Types:** 4  
**Error Codes:** 25+  
**HTTP Status Codes:** 5  

**Coverage:**
- ✅ Controllers (via Global Handler)
- ✅ Services (all methods)
- ✅ Repositories (all operations)
- ✅ Middleware (global catch-all)

**Features:**
- ✅ Structured error responses
- ✅ Contextual logging
- ✅ Audit trail integration
- ✅ Security-conscious messages
- ✅ Client-friendly error codes

---

**Exception handling complete! Har jagah proper try-catch aur error handling implemented hai!** 🛡️✨
