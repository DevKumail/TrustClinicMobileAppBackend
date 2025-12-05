# 🏥 Coherent Mobile - Patient Signup Flow

## 📋 Overview
Patient signup flow for Fertility Clinic using QR code scan, OTP verification, and profile creation.

---

## 🔄 Complete Signup Flow

### **Step 1: QR Code Scan**
**API:** `GET /api/auth/qr-scan/{qrCodeId}`

**Process:**
1. Patient visits Fertility Clinic
2. Scans QR code at reception
3. System retrieves patient data from clinic database

**Response:**
```json
{
  "mrno": "MRN001234",
  "fullName": "Ahmed Mohammed Ali",
  "dateOfBirth": "1990-05-15",
  "emiratesIdType": "Emirates"
}
```

**Database:** Record saved in `QRCodeScans` table

---

### **Step 2: Verify Information**
**API:** `POST /api/auth/verify-information`

**Request Body (Emirates ID):**
```json
{
  "mrno": "MRN001234",
  "emiratesId": "784-1990-1234567-1",
  "mobileNumber": "+971501234567"
}
```

**Request Body (Passport):**
```json
{
  "mrno": "MRN001234",
  "passportNumber": "P12345678",
  "mobileNumber": "+971501234567",
  "email": "patient@example.com"
}
```

**Process:**
1. Validate user information
2. Generate 6-digit OTP
3. Send OTP to mobile/email
4. Save OTP in `OTPVerifications` table with 5-minute expiry

**Response:**
```json
{
  "success": true,
  "message": "OTP sent successfully",
  "deliveryChannel": "SMS",
  "expiresIn": 300
}
```

---

### **Step 3: OTP Verification**
**API:** `POST /api/auth/verify-otp`

**Request Body:**
```json
{
  "mrno": "MRN001234",
  "otp": "123456"
}
```

**Process:**
1. Verify OTP code
2. Check if not expired (5 minutes)
3. Check attempt count (max 3)
4. Mark OTP as verified

**Response:**
```json
{
  "success": true,
  "message": "OTP verified successfully",
  "token": "temp_token_for_profile_creation"
}
```

---

### **Step 4: Create Profile (Set Password)**
**API:** `POST /api/auth/create-profile`

**Request Body:**
```json
{
  "mrno": "MRN001234",
  "password": "SecureP@ssw0rd!",
  "confirmPassword": "SecureP@ssw0rd!"
}
```

**Process:**
1. Hash password with salt (BCrypt)
2. Create patient record in `Patients` table
3. Mark mobile/email as verified
4. Set profile as complete
5. Create audit log

**Response:**
```json
{
  "success": true,
  "message": "Profile created successfully",
  "patientId": "guid",
  "accessToken": "jwt_token_here",
  "refreshToken": "refresh_token_here"
}
```

---

## 🔐 Sign-In Flow

### **Login API**
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "identifier": "784-1990-1234567-1",
  "password": "SecureP@ssw0rd!"
}
```
*Note: `identifier` can be Emirates ID or Passport Number*

**Process:**
1. Find patient by Emirates ID or Passport
2. Verify password hash
3. Check if account is locked
4. Reset failed login attempts on success
5. Create login session
6. Generate JWT tokens

**Response:**
```json
{
  "success": true,
  "patientId": "guid",
  "fullName": "Ahmed Mohammed Ali",
  "mrno": "MRN001234",
  "accessToken": "jwt_token_here",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600
}
```

---

## 🔑 Forgot Password Flow

### **Step 1: Request Password Reset**
**API:** `POST /api/auth/forgot-password`

**Request Body:**
```json
{
  "email": "patient@example.com"
}
```

**Process:**
1. Find patient by email
2. Generate secure reset token
3. Save token in `PasswordResetTokens` table
4. Send reset link to email
5. Token expires in 30 minutes

**Response:**
```json
{
  "success": true,
  "message": "Password reset link sent to your email"
}
```

---

### **Step 2: Reset Password**
**API:** `POST /api/auth/reset-password`

**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "NewP@ssw0rd!",
  "confirmPassword": "NewP@ssw0rd!"
}
```

**Process:**
1. Validate reset token
2. Check if not expired/used
3. Hash new password
4. Update patient password
5. Mark token as used
6. Create audit log

---

## 📊 Database Tables

### **1. Patients Table**
Main patient/user table with authentication details.

**Key Fields:**
- `MRNO` - Medical Record Number (unique)
- `EmiratesIdType` - 'Emirates' or 'Passport'
- `EmiratesId` - For UAE Nationals
- `PassportNumber` - For Expats
- `MobileNumber` - Required
- `Email` - Required for passport holders
- `PasswordHash` & `PasswordSalt` - Hashed credentials
- `IsProfileComplete` - Signup flow completed
- `IsLocked` - Account locked after 5 failed attempts

---

### **2. OTPVerifications Table**
Tracks all OTP codes sent for signup, login, and password reset.

**Key Fields:**
- `OTPCode` - 6-digit code
- `OTPType` - 'Signup', 'Login', 'ForgotPassword'
- `DeliveryChannel` - 'SMS' or 'Email'
- `ExpiresAt` - 5 minutes from creation
- `AttemptCount` - Max 3 attempts
- `IsVerified` - OTP successfully verified

---

### **3. QRCodeScans Table**
Audit trail for QR code scans.

**Key Fields:**
- `QRCodeId` - Scanned QR identifier
- `MRNO` - Patient medical record
- `ScannedAt` - Timestamp
- `IsSignupCompleted` - Flow completion status

---

### **4. PasswordResetTokens Table**
Manages password reset tokens.

**Key Fields:**
- `Token` - Secure reset token
- `ExpiresAt` - 30 minutes validity
- `IsUsed` - Token usage status
- `IsExpired` - Expiration status

---

### **5. LoginSessions Table**
Tracks active user sessions and JWT tokens.

**Key Fields:**
- `AccessToken` - JWT access token
- `RefreshToken` - Refresh token for renewal
- `ExpiresAt` - Token expiration
- `IsActive` - Session active status
- `DeviceInfo` - Device/browser info

---

### **6. AuthAuditLogs Table**
Complete audit trail for all authentication actions.

**Key Fields:**
- `Action` - QRScan, OTPSent, Login, etc.
- `Status` - Success, Failed, Pending
- `Details` - Additional information
- `IPAddress` - Request IP
- `CreatedAt` - Action timestamp

---

## 🔒 Security Features

1. **Password Hashing:** BCrypt with salt
2. **OTP Expiry:** 5 minutes
3. **OTP Attempts:** Maximum 3 tries
4. **Account Locking:** After 5 failed login attempts
5. **Token Expiry:** JWT tokens expire in 1 hour
6. **Audit Logging:** All actions logged
7. **Soft Delete:** Data retained for audit

---

## 📈 Stored Procedures

### **sp_GetPatientByIdentity**
Get patient by Emirates ID or Passport Number

### **sp_VerifyOTP**
Verify OTP code with attempt tracking

### **sp_UpdateFailedLoginAttempts**
Track failed login attempts

### **sp_ResetFailedLoginAttempts**
Reset on successful login

---

## 📱 API Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (validation failed) |
| 401 | Unauthorized (invalid credentials) |
| 403 | Forbidden (account locked) |
| 404 | Not Found |
| 429 | Too Many Requests (rate limit) |
| 500 | Server Error |

---

## ✅ Validation Rules

### **Password:**
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

### **Mobile Number:**
- Valid UAE format: +971XXXXXXXXX
- 10 digits after country code

### **Emirates ID:**
- Format: 784-YYYY-NNNNNNN-C
- Valid checksum

### **Passport:**
- Alphanumeric
- 6-12 characters

---

## 🚀 Next Steps

1. Run `CreateUserAuthTables.sql` to create database schema
2. Implement repository interfaces for new entities
3. Create DTOs for API requests/responses
4. Implement authentication services
5. Add OTP provider (SMS/Email integration)
6. Implement JWT token generation
7. Add validation rules
8. Create API controllers
9. Add unit tests
10. Test complete signup flow

---

**Created by:** Coherent Mobile Development Team  
**Last Updated:** December 2024
