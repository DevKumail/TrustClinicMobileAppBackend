# Coherent Mobile Health Application

## 🏥 Overview

Coherent is an enterprise-grade mobile health application built with .NET 8 Web API following Clean Architecture principles. It provides a robust foundation for managing health records, real-time communication, and third-party API integrations.

## 🏗️ Architecture

This project implements **Clean Architecture** with clear separation of concerns across five layers:

### Project Structure

```
Coherent/
├── src/
│   ├── CoherentMobile.API/                    # Presentation Layer
│   │   ├── Controllers/                 # REST API Controllers
│   │   ├── Hubs/                        # SignalR Hubs
│   │   ├── Middleware/                  # Custom Middleware
│   │   └── Program.cs                   # Application Entry Point
│   │
│   ├── CoherentMobile.Application/            # Application Layer
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Interfaces/                  # Service Interfaces
│   │   ├── Services/                    # Business Logic Services
│   │   └── Validators/                  # Fluent Validation Rules
│   │
│   ├── CoherentMobile.Domain/                 # Domain Layer
│   │   ├── Entities/                    # Domain Entities
│   │   └── Interfaces/                  # Repository Interfaces
│   │
│   ├── CoherentMobile.Infrastructure/         # Infrastructure Layer
│   │   ├── Data/                        # Database Context
│   │   └── Repositories/                # Dapper Repository Implementations
│   │
│   └── CoherentMobile.ExternalIntegration/    # External Integration Layer
│       ├── Clients/                     # Third-Party API Clients
│       ├── Interfaces/                  # Integration Interfaces
│       └── Models/                      # External API Models
│
└── Database/                            # Database Scripts
```

## 🚀 Features

### Core Technologies

- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **Serilog Logging** - Structured logging to console and file
- ✅ **SignalR** - Real-time bi-directional communication
- ✅ **CORS** - Configured cross-origin resource sharing
- ✅ **Dapper ORM** - High-performance data access
- ✅ **Fluent Validation** - Comprehensive input validation
- ✅ **Swagger/OpenAPI** - Interactive API documentation

### Architecture Patterns

- **Clean Architecture** - Separation of concerns with dependency inversion
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Dependency Injection** - Loose coupling and testability
- **Middleware Pipeline** - Request/response processing
- **CQRS-ready** - Separated command and query concerns

## 🛠️ Getting Started

### Prerequisites

- .NET 8 SDK or later
- SQL Server 2019 or later (or SQL Server Express)
- Visual Studio 2022 / VS Code / Rider
- Postman or similar API testing tool (optional)

### Installation

1. **Clone the repository**
   ```bash
   cd "c:\Users\DELL\Desktop\Coheret\Coherent Mobile\Coherent Mobile"
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore CoherentMobile.sln
   ```

3. **Update connection string**
   
   Edit `src/CoherentMobile.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=CoherentHealthDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
   }
   ```

4. **Create database**
   
   Run the SQL script in `Database/CreateDatabase.sql` to create the database schema.

5. **Update JWT secret**
   
   In `appsettings.json`, update the JWT secret (minimum 32 characters):
   ```json
   "Jwt": {
     "Secret": "YOUR_SECURE_SECRET_KEY_HERE_MINIMUM_32_CHARACTERS"
   }
   ```

6. **Run the application**
   ```bash
   cd src/CoherentMobile.API
   dotnet run
   ```

7. **Access Swagger UI**
   
   Open browser: `https://localhost:5001` or `http://localhost:5000`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/validate-token` - Validate JWT token

### User Management
- `GET /api/user/profile` - Get current user profile
- `PUT /api/user/profile` - Update user profile
- `GET /api/user` - Get all users
- `DELETE /api/user/deactivate` - Deactivate account

### Health Records
- `POST /api/healthrecord` - Create health record
- `GET /api/healthrecord` - Get all user health records
- `GET /api/healthrecord/type/{recordType}` - Get records by type
- `GET /api/healthrecord/date-range` - Get records by date range
- `DELETE /api/healthrecord/{recordId}` - Delete health record

### External Integration
- `GET /api/integration/external-health-data/{dataType}` - Fetch external health data
- `POST /api/integration/sync-health-data` - Sync to external API
- `POST /api/integration/send-notification` - Send notification via external service

### SignalR Hub
- **Endpoint**: `/hubs/healthdata`
- **Methods**:
  - `SubscribeToHealthUpdates(healthDataType)` - Subscribe to real-time updates
  - `SendHealthDataUpdate(userId, healthData)` - Send update to user
  - `BroadcastHealthAlert(message, severity)` - Broadcast alert

## 🔐 Authentication

### Register a User

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "gender": "Male",
  "dateOfBirth": "1990-01-01"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecureP@ssw0rd!"
}
```

Response includes JWT token to use in subsequent requests:
```json
{
  "userId": "guid",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-02T12:00:00Z"
}
```

### Using the Token

Add to request headers:
```
Authorization: Bearer {your-token-here}
```

## 🔌 SignalR Real-Time Communication

### JavaScript Client Example

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/hubs/healthdata", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

// Connect
await connection.start();

// Subscribe to updates
await connection.invoke("SubscribeToHealthUpdates", "HeartRate");

// Listen for updates
connection.on("ReceiveHealthDataUpdate", (data) => {
    console.log("Health data update:", data);
});

// Listen for alerts
connection.on("ReceiveHealthAlert", (alert) => {
    console.log("Health alert:", alert);
});
```

## 📊 Database Schema

The application uses SQL Server with the following main tables:

- **Users** - User accounts and profiles
- **HealthRecords** - Health data records

See `Database/CreateDatabase.sql` for complete schema.

## 🧪 Testing

### Using Swagger
1. Navigate to `https://localhost:5001`
2. Click "Authorize" button
3. Enter: `Bearer {your-token}`
4. Test endpoints interactively

### Using Postman
Import the Swagger JSON and test all endpoints with the Postman collection.

## 🌐 External API Integration

The **External Integration Layer** is designed for calling third-party APIs:

- **Health Data API Client** - Fetch and sync health data from external sources
- **Notification API Client** - Send emails, SMS, and push notifications

Configure in `appsettings.json`:
```json
"ExternalApis": {
  "HealthDataApi": {
    "BaseUrl": "https://api.healthdata.example.com",
    "ApiKey": "your-api-key"
  },
  "NotificationApi": {
    "BaseUrl": "https://api.notifications.example.com",
    "ApiKey": "your-api-key"
  }
}
```

## 📝 Logging

Serilog is configured to log to:
- **Console** - Colored structured logs
- **File** - Rolling daily logs in `logs/` directory

Log levels can be configured in `appsettings.json`.

## 🔧 Configuration

### CORS Origins

Update allowed origins in `appsettings.json`:
```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://yourdomain.com"
  ]
}
```

### JWT Settings

```json
"Jwt": {
  "Secret": "your-secret-key",
  "Issuer": "CoherentHealthAPI",
  "Audience": "CoherentMobileApp",
  "ExpiryHours": 24
}
```

## 🚀 Deployment

### Production Checklist

- [ ] Update JWT secret with strong random key
- [ ] Update database connection string
- [ ] Set `RequireHttpsMetadata = true` in JWT configuration
- [ ] Configure production CORS origins
- [ ] Set up production logging (Application Insights, etc.)
- [ ] Enable HTTPS redirection
- [ ] Configure external API keys
- [ ] Set up database migrations
- [ ] Review and harden security settings

## 📦 NuGet Packages

### API Project
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.SignalR
- Serilog.AspNetCore
- Swashbuckle.AspNetCore

### Application Project
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

### Infrastructure Project
- Dapper
- System.Data.SqlClient

### External Integration Project
- Microsoft.Extensions.Http.Polly
- Polly (for retry policies)

## 🤝 Contributing

This is a boilerplate template. Customize as needed for your specific requirements.

## 📄 License

This project is provided as-is for use in the Coherent mobile health application.

## 📞 Support

For issues or questions, contact the development team.

---

**Built with ❤️ using Clean Architecture and .NET 8**
