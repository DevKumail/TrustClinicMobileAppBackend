# 🔐 Coherent Mobile - Authentication API Testing Guide

## ✅ **Implementation Complete!**

### **What Has Been Created:**

#### 📁 **1. DTOs (Data Transfer Objects)**
- `QRScanResponseDto` - QR code scan response from web portal
- `VerifyInformationRequestDto` / `ResponseDto` - Step 2: Verify user info
- `VerifyOTPRequestDto` / `ResponseDto` - Step 3: OTP verification
- `CreateProfileRequestDto` / `ResponseDto` - Step 4: Profile creation
- `LoginRequestDto` / `ResponseDto` - User login
- `ForgotPasswordRequestDto` / `ResponseDto` - Password reset request
- `ResetPasswordRequestDto` / `ResponseDto` - Password reset completion

#### ✔️ **2. FluentValidation Validators**
- `VerifyInformationRequestValidator` - Validates Emirates ID/Passport format
- `VerifyOTPRequestValidator` - Validates 6-digit OTP
- `CreateProfileRequestValidator` - Password strength validation
- `LoginRequestValidator` - Login credentials validation
- `ForgotPasswordRequestValidator` - Email validation
- `ResetPasswordRequestValidator` - Password reset validation

#### 🔧 **3. Helper Services**
- `PasswordHasher` - PBKDF2 password hashing with salt
- `JwtTokenGenerator` - JWT access & refresh token generation
- `OTPGenerator` - Secure 6-digit OTP generation

#### 🏢 **4. Business Logic Services**
- `AuthenticationService` - Complete signup/login flow
- `SMSService` (stub) - SMS sending interface
- `EmailService` (stub) - Email sending interface

#### 🎮 **5. API Controller**
- `AuthenticationController` - All authentication endpoints

---

## 🔄 **Complete Signup Flow**

### **Step 1: QR Scan (Web Portal - External)**
🌐 User scans QR at clinic → Web portal returns patient data

**Response from Web Portal:**
```json
{
  "mrno": "MRN001234",
  "fullName": "Ahmed Mohammed Ali",
  "dateOfBirth": "1990-05-15",
  "emiratesIdType": "Emirates"
}
```

---

### **Step 2: Verify Information + Send OTP**

**Endpoint:** `POST /api/authentication/verify-information`

**Request Body (Emirates ID):**
```json
{
  "mrno": "MRN001234",
  "emiratesId": "784-1990-1234567-1",
  "mobileNumber": "+971501234567",
  "deliveryChannel": "SMS"
}
```

**Request Body (Passport):**
```json
{
  "mrno": "MRN001235",
  "passportNumber": "P12345678",
  "mobileNumber": "+971509876543",
  "email": "john@example.com",
  "deliveryChannel": "Email"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "OTP sent successfully",
  "deliveryChannel": "SMS",
  "expiresIn": 300,
  "recipientContact": "+97150***67"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Emirates ID already registered"
}
```

---

### **Step 3: Verify OTP**

**Endpoint:** `POST /api/authentication/verify-otp`

**Request Body:**
```json
{
  "mrno": "MRN001234",
  "otpCode": "123456"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "OTP verified successfully",
  "tempToken": "dGVtcF90b2tlbl9oZXJl"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Invalid or expired OTP"
}
```

---

### **Step 4: Create Profile (Set Password)**

**Endpoint:** `POST /api/authentication/create-profile`

**Request Body:**
```json
{
  "mrno": "MRN001234",
  "password": "SecureP@ssw0rd!",
  "confirmPassword": "SecureP@ssw0rd!"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Profile created successfully",
  "patientId": 123,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 86400
}
```

**Password Requirements:**
- ✅ Minimum 8 characters
- ✅ At least 1 uppercase letter (A-Z)
- ✅ At least 1 lowercase letter (a-z)
- ✅ At least 1 number (0-9)
- ✅ At least 1 special character (@$!%*?&#)

---

## 🔑 **Login Flow**

**Endpoint:** `POST /api/authentication/login`

**Request Body:**
```json
{
  "identifier": "784-1990-1234567-1",
  "password": "SecureP@ssw0rd!"
}
```
*Note: `identifier` can be Emirates ID or Passport Number*

**Response (Success):**
```json
{
  "success": true,
  "message": "Login successful",
  "patientId": 123,
  "mrno": "MRN001234",
  "fullName": "Ahmed Mohammed Ali",
  "mobileNumber": "+971501234567",
  "email": null,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 86400
}
```

**Response (Error - Invalid Credentials):**
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

**Response (Error - Account Locked):**
```json
{
  "success": false,
  "message": "Account is locked due to multiple failed login attempts. Please contact support."
}
```

---

## 🔐 **Forgot Password Flow**

### **Step 1: Request Password Reset**

**Endpoint:** `POST /api/authentication/forgot-password`

**Request Body:**
```json
{
  "email": "patient@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "If the email exists, a password reset link has been sent."
}
```

---

### **Step 2: Reset Password**

**Endpoint:** `POST /api/authentication/reset-password`

**Request Body:**
```json
{
  "identifier": "784-1990-1234567-1",
  "newPassword": "NewP@ssw0rd!",
  "confirmPassword": "NewP@ssw0rd!"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Password reset successfully. Please login with your new password."
}
```

---

## 🧪 **Postman Testing Steps**

### **1. Setup Environment Variables**
```
base_url: https://localhost:7162
access_token: (will be set automatically after login/signup)
```

### **2. Test Signup Flow**

#### **A. Verify Information (Send OTP)**
```http
POST {{base_url}}/api/authentication/verify-information
Content-Type: application/json

{
  "mrno": "MRN001234",
  "emiratesId": "784-1990-1234567-1",
  "mobileNumber": "+971501234567",
  "deliveryChannel": "SMS"
}
```

#### **B. Check Console Logs for OTP**
The OTP will be logged in the console since we're using stub services.

#### **C. Verify OTP**
```http
POST {{base_url}}/api/authentication/verify-otp
Content-Type: application/json

{
  "mrno": "MRN001234",
  "otpCode": "123456"
}
```

#### **D. Create Profile**
```http
POST {{base_url}}/api/authentication/create-profile
Content-Type: application/json

{
  "mrno": "MRN001234",
  "password": "SecureP@ssw0rd!",
  "confirmPassword": "SecureP@ssw0rd!"
}
```

**Save the `accessToken` from response!**

---

### **3. Test Login**
```http
POST {{base_url}}/api/authentication/login
Content-Type: application/json

{
  "identifier": "784-1990-1234567-1",
  "password": "SecureP@ssw0rd!"
}
```

---

### **4. Test Forgot Password**
```http
POST {{base_url}}/api/authentication/forgot-password
Content-Type: application/json

{
  "email": "patient@example.com"
}
```

---

## 🔍 **Common Validation Errors**

### **Emirates ID Format**
✅ Valid: `784-1990-1234567-1`  
❌ Invalid: `7841990123456` (missing dashes)

### **Mobile Number Format**
✅ Valid: `+971501234567`  
❌ Invalid: `0501234567` (missing country code)

### **Password Strength**
✅ Valid: `SecureP@ssw0rd!`  
❌ Invalid: `password123` (no uppercase, no special char)

### **OTP Code**
✅ Valid: `123456` (6 digits)  
❌ Invalid: `12345` (too short)

---

## 📊 **Database Tables Used**

| Table | Purpose |
|-------|---------|
| `Users` | Patient/User records |
| `OTPVerifications` | OTP codes with expiry |
| `AuthAuditLogs` | Complete audit trail |
| `PasswordResetTokens` | Password reset tokens |

---

## 🔒 **Security Features**

1. ✅ **Password Hashing** - PBKDF2 with 100,000 iterations
2. ✅ **OTP Expiry** - 5 minutes
3. ✅ **Max OTP Attempts** - 3 tries
4. ✅ **Account Locking** - After 5 failed login attempts
5. ✅ **JWT Tokens** - Secure authentication
6. ✅ **Audit Logging** - All actions logged
7. ✅ **Soft Delete** - Data retained for compliance

---

## 📝 **Next Steps**

### **For Production:**
1. **Integrate Real SMS Provider:**
   - Update `CoherentMobile.Infrastructure/Services/SMSService.cs`
   - Add Twilio, AWS SNS, or similar

2. **Integrate Real Email Provider:**
   - Update `CoherentMobile.Infrastructure/Services/EmailService.cs`
   - Add SendGrid, AWS SES, or similar

3. **Replace In-Memory QR Cache:**
   - Use Redis or database temporary storage
   - Current: `Dictionary<string, QRScanResponseDto>`

4. **Add Rate Limiting:**
   - Prevent OTP spam
   - Prevent brute force login

5. **Add Refresh Token Logic:**
   - Implement token refresh endpoint
   - Store refresh tokens in database

6. **Production Connection String:**
   - Already configured in `appsettings.json`
   - Server: 175.107.195.221
   - Database: CoherentMobApp

---

## 🚀 **Ready to Test!**

Run the application:
```bash
dotnet run --project CoherentMobile.Api
```

Swagger URL:
```
https://localhost:7162/swagger
```

---

**All APIs are ready for testing!** 🎉

**Created:** Complete authentication flow  
**Database:** CoherentMobApp (Production)  
**Framework:** .NET 8 Web API  
**Architecture:** Clean Architecture with Dapper ORM
