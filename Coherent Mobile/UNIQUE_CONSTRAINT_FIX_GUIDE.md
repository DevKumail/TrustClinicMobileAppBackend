# 🛠️ UNIQUE Constraint NULL Issue - Complete Fix Guide

## 🐛 **Problem:**

**Error:** `Violation of UNIQUE KEY constraint. Cannot insert duplicate key. The duplicate key value is (<NULL>).`

### **Root Cause:**
SQL Server **UNIQUE constraints** only allow **ONE NULL value**. When you have:
- User 1: EmiratesId = "784-1990-xxx", PassportNumber = NULL
- User 2: EmiratesId = NULL, PassportNumber = "P12345" ❌ **FAILS** (second NULL not allowed)

---

## ✅ **Solution: Filtered UNIQUE Indexes**

### **What Changed:**
- ❌ **Old:** UNIQUE constraints (only 1 NULL allowed)
- ✅ **New:** Filtered UNIQUE indexes (multiple NULLs allowed)

---

## 📋 **Step 1: Run SQL Fix Script**

### **Option A: Run Complete Script**
```sql
-- File: CoherentMobile.Api/Database/FixUniqueConstraints.sql
```

### **Option B: Quick SQL Fix**

```sql
USE CoherentMobApp;
GO

-- =============================================
-- Step 1: Drop ALL existing UNIQUE constraints
-- =============================================
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + 'ALTER TABLE Users DROP CONSTRAINT ' + name + ';' + CHAR(13)
FROM sys.key_constraints
WHERE type = 'UQ' AND parent_object_id = OBJECT_ID('Users');

PRINT 'Dropping constraints:';
PRINT @sql;
EXEC sp_executesql @sql;
GO

-- =============================================
-- Step 2: Create Filtered UNIQUE Indexes
-- =============================================

-- MRNO: Always required, normal unique index
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_MRNO' AND object_id = OBJECT_ID('Users'))
    DROP INDEX UQ_Users_MRNO ON Users;
CREATE UNIQUE INDEX UQ_Users_MRNO ON Users(MRNO);
PRINT '✅ Created UNIQUE index on MRNO';

-- EmiratesId: Allow multiple NULLs
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_EmiratesId' AND object_id = OBJECT_ID('Users'))
    DROP INDEX UQ_Users_EmiratesId ON Users;
CREATE UNIQUE INDEX UQ_Users_EmiratesId 
ON Users(EmiratesId) 
WHERE EmiratesId IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on EmiratesId';

-- PassportNumber: Allow multiple NULLs
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_PassportNumber' AND object_id = OBJECT_ID('Users'))
    DROP INDEX UQ_Users_PassportNumber ON Users;
CREATE UNIQUE INDEX UQ_Users_PassportNumber 
ON Users(PassportNumber) 
WHERE PassportNumber IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on PassportNumber';

-- Email: Allow multiple NULLs (optional field)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_Email' AND object_id = OBJECT_ID('Users'))
    DROP INDEX UQ_Users_Email ON Users;
CREATE UNIQUE INDEX UQ_Users_Email 
ON Users(Email) 
WHERE Email IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on Email';

-- MobileNumber: Allow multiple NULLs
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_MobileNumber' AND object_id = OBJECT_ID('Users'))
    DROP INDEX UQ_Users_MobileNumber ON Users;
CREATE UNIQUE INDEX UQ_Users_MobileNumber 
ON Users(MobileNumber) 
WHERE MobileNumber IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on MobileNumber';

GO

PRINT '';
PRINT '========================================';
PRINT '✅ UNIQUE Constraint Fix COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'Changes:';
PRINT '- Dropped old UNIQUE constraints';
PRINT '- Created filtered UNIQUE indexes';
PRINT '- Now allows multiple NULL values';
PRINT '';
GO

-- =============================================
-- Step 3: Verify Indexes
-- =============================================
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.filter_definition AS FilterDefinition,
    c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('Users')
AND i.is_unique = 1
ORDER BY i.name;
GO
```

---

## 🔧 **Step 2: Code Improvements (Already Done)**

### **✅ Enhanced Error Handling**

Updated `GenericRepository.AddAsync()` to detect UNIQUE violations:

```csharp
catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
{
    var errorMessage = "Duplicate entry detected. ";
    if (ex.Message.Contains("EmiratesId"))
        errorMessage += "Emirates ID already exists.";
    else if (ex.Message.Contains("PassportNumber"))
        errorMessage += "Passport number already exists.";
    // ... more specific messages
    
    throw new DatabaseException(errorMessage, ex, "DB_DUPLICATE_ERROR");
}
```

### **✅ Improved Column Selection**

Updated `GetColumns()` to properly handle nullable types:

```csharp
protected virtual IEnumerable<string> GetColumns(bool excludeKey = false)
{
    var type = typeof(T);
    var columns = type.GetProperties()
        .Where(p => !excludeKey || p.Name != "Id")
        .Where(p => p.PropertyType.IsValueType || 
                   p.PropertyType == typeof(string) ||
                   Nullable.GetUnderlyingType(p.PropertyType) != null)
        .Where(p => !typeof(IEnumerable).IsAssignableFrom(p.PropertyType) || 
                   p.PropertyType == typeof(string))
        .Select(p => p.Name);

    return columns;
}
```

### **✅ Fixed Signup Cache**

Stores complete signup data for profile creation:

```csharp
private class SignupCacheData
{
    public QRScanResponseDto QRData { get; set; }
    public VerifyInformationRequestDto VerifyRequest { get; set; }
    public DateTime CachedAt { get; set; }
}
```

---

## 🧪 **Step 3: Test the Fix**

### **Test Case 1: Emirates User**
```json
POST /api/authentication/verify-information
{
  "mrno": "MRN001250",
  "emiratesId": "784-1995-8888888-8",
  "mobileNumber": "+971552223333",
  "deliveryChannel": "SMS"
}

// Then OTP → Create Profile
// Database Insert:
// EmiratesId = "784-1995-8888888-8" ✅
// PassportNumber = NULL ✅ (allowed)
```

### **Test Case 2: Passport User**
```json
POST /api/authentication/verify-information
{
  "mrno": "MRN001251",
  "passportNumber": "P87654321",
  "mobileNumber": "+971559876543",
  "deliveryChannel": "SMS"
}

// Database Insert:
// EmiratesId = NULL ✅ (allowed)
// PassportNumber = "P87654321" ✅
```

### **Test Case 3: Multiple NULL Users**
```
User 1: EmiratesId = "XXX", PassportNumber = NULL, Email = NULL ✅
User 2: EmiratesId = NULL, PassportNumber = "YYY", Email = NULL ✅
User 3: EmiratesId = "ZZZ", PassportNumber = NULL, Email = "test@x.com" ✅

All succeed! Multiple NULLs allowed
```

---

## 📊 **Before vs After:**

### **❌ Before (UNIQUE Constraints):**
```sql
CREATE TABLE Users (
    EmiratesId NVARCHAR(50) NULL UNIQUE,
    PassportNumber NVARCHAR(50) NULL UNIQUE,
    Email NVARCHAR(200) NULL UNIQUE
);

-- Only ONE NULL allowed per column!
INSERT: EmiratesId = NULL → ✅ Success
INSERT: EmiratesId = NULL → ❌ FAILS!
```

### **✅ After (Filtered Indexes):**
```sql
CREATE UNIQUE INDEX UQ_Users_EmiratesId 
ON Users(EmiratesId) 
WHERE EmiratesId IS NOT NULL;

CREATE UNIQUE INDEX UQ_Users_PassportNumber 
ON Users(PassportNumber) 
WHERE PassportNumber IS NOT NULL;

-- Multiple NULLs allowed!
INSERT: EmiratesId = NULL, PassportNumber = "P1" → ✅ Success
INSERT: EmiratesId = NULL, PassportNumber = "P2" → ✅ Success
INSERT: EmiratesId = "E1", PassportNumber = NULL → ✅ Success
INSERT: EmiratesId = "E2", PassportNumber = NULL → ✅ Success
```

---

## 🎯 **Key Differences:**

| Feature | UNIQUE Constraint | Filtered UNIQUE Index |
|---------|-------------------|----------------------|
| **NULL Handling** | Only 1 NULL allowed | Multiple NULLs allowed |
| **Syntax** | `UNIQUE` | `WHERE column IS NOT NULL` |
| **Performance** | Same | Same |
| **Use Case** | Required fields | Optional fields |

---

## ✅ **Verification Steps:**

### **1. Check Indexes:**
```sql
-- See all unique indexes
SELECT 
    i.name AS IndexName,
    c.name AS ColumnName,
    i.filter_definition AS Filter
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('Users')
AND i.is_unique = 1;
```

Expected Output:
```
IndexName                    ColumnName       Filter
---------------------------  ---------------  --------------------------
UQ_Users_MRNO                MRNO             NULL
UQ_Users_EmiratesId          EmiratesId       ([EmiratesId] IS NOT NULL)
UQ_Users_PassportNumber      PassportNumber   ([PassportNumber] IS NOT NULL)
UQ_Users_Email               Email            ([Email] IS NOT NULL)
UQ_Users_MobileNumber        MobileNumber     ([MobileNumber] IS NOT NULL)
```

### **2. Test Duplicate Detection:**
```sql
-- Should PASS (different values)
INSERT INTO Users (...) VALUES ('E1', NULL, ...);
INSERT INTO Users (...) VALUES (NULL, 'P1', ...);

-- Should FAIL (duplicate EmiratesId)
INSERT INTO Users (...) VALUES ('E1', NULL, ...);
-- Error: "Emirates ID already exists"

-- Should FAIL (duplicate PassportNumber)
INSERT INTO Users (...) VALUES (NULL, 'P1', ...);
-- Error: "Passport number already exists"
```

---

## 🚀 **Quick Deployment Steps:**

1. ✅ **Backup Database** (important!)
   ```sql
   BACKUP DATABASE CoherentMobApp TO DISK = 'backup.bak';
   ```

2. ✅ **Run SQL Fix Script**
   - Execute `FixUniqueConstraints.sql`
   - Or run the quick SQL fix above

3. ✅ **Restart Application**
   ```bash
   dotnet run --project CoherentMobile.Api
   ```

4. ✅ **Test Signup Flow**
   - Create Emirates user
   - Create Passport user
   - Verify both succeed

---

## 📝 **Summary:**

**Problem:** UNIQUE constraints only allow ONE NULL value  
**Solution:** Filtered UNIQUE indexes allow multiple NULLs  
**Impact:** Users can now signup with either Emirates ID or Passport  
**Status:** ✅ **FIXED**

---

## 🎉 **Result:**

- ✅ Multiple users with NULL EmiratesId (Passport users)
- ✅ Multiple users with NULL PassportNumber (Emirates users)
- ✅ Multiple users with NULL Email (SMS-only users)
- ✅ Still prevents actual duplicates (same Emirates ID, etc.)
- ✅ Better error messages for duplicate detection

**Database fix complete! Ab sab kuch kaam karega!** 🚀✨
