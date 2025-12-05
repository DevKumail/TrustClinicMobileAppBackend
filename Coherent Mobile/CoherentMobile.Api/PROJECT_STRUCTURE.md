# Coherent Mobile Health - Project Structure

## 📁 Complete File Structure

```
Coherent Mobile/
│
├── CoherentMobile.sln                          # Solution file
├── README.md                             # Main documentation
├── ARCHITECTURE.md                       # Architecture details
├── QUICK_START.md                        # Quick start guide
├── .gitignore                            # Git ignore rules
│
├── Database/
│   └── CreateDatabase.sql                # Database schema script
│
└── src/
    │
    ├── CoherentMobile.API/                     # 🌐 Presentation Layer
    │   ├── Controllers/
    │   │   ├── AuthController.cs         # Authentication endpoints
    │   │   ├── UserController.cs         # User management endpoints
    │   │   ├── HealthRecordController.cs # Health records endpoints
    │   │   └── IntegrationController.cs  # External API integration endpoints
    │   │
    │   ├── Hubs/
    │   │   └── HealthDataHub.cs          # SignalR real-time hub
    │   │
    │   ├── Middleware/
    │   │   ├── ErrorHandlingMiddleware.cs      # Global error handler
    │   │   └── RequestLoggingMiddleware.cs     # Request/response logger
    │   │
    │   ├── Program.cs                    # Application entry point & DI config
    │   ├── appsettings.json              # Application configuration
    │   ├── appsettings.Development.json  # Development configuration
    │   └── CoherentMobile.API.csproj           # API project file
    │
    ├── CoherentMobile.Application/             # 📋 Application Layer
    │   ├── DTOs/
    │   │   ├── UserDtos.cs               # User DTOs (Register, Login, Profile)
    │   │   └── HealthRecordDtos.cs       # Health record DTOs
    │   │
    │   ├── Interfaces/
    │   │   ├── IAuthService.cs           # Authentication service interface
    │   │   ├── IUserService.cs           # User service interface
    │   │   └── IHealthRecordService.cs   # Health record service interface
    │   │
    │   ├── Services/
    │   │   ├── AuthService.cs            # Authentication implementation
    │   │   ├── UserService.cs            # User management implementation
    │   │   └── HealthRecordService.cs    # Health records implementation
    │   │
    │   ├── Validators/
    │   │   ├── RegisterUserValidator.cs        # User registration validation
    │   │   ├── LoginValidator.cs               # Login validation
    │   │   └── CreateHealthRecordValidator.cs  # Health record validation
    │   │
    │   ├── DependencyInjection.cs        # Application layer DI setup
    │   └── CoherentMobile.Application.csproj   # Application project file
    │
    ├── CoherentMobile.Domain/                  # 🏛️ Domain Layer (Core)
    │   ├── Entities/
    │   │   ├── BaseEntity.cs             # Base entity with common properties
    │   │   ├── User.cs                   # User entity
    │   │   └── HealthRecord.cs           # Health record entity
    │   │
    │   ├── Interfaces/
    │   │   ├── IRepository.cs            # Generic repository interface
    │   │   ├── IUserRepository.cs        # User repository interface
    │   │   ├── IHealthRecordRepository.cs # Health record repository interface
    │   │   └── IUnitOfWork.cs            # Unit of Work interface
    │   │
    │   └── CoherentMobile.Domain.csproj        # Domain project file
    │
    ├── CoherentMobile.Infrastructure/          # 🔧 Infrastructure Layer
    │   ├── Data/
    │   │   └── DapperContext.cs          # Dapper database context
    │   │
    │   ├── Repositories/
    │   │   ├── BaseRepository.cs         # Generic repository implementation
    │   │   ├── UserRepository.cs         # User repository (Dapper)
    │   │   ├── HealthRecordRepository.cs # Health record repository (Dapper)
    │   │   └── UnitOfWork.cs             # Unit of Work implementation
    │   │
    │   ├── DependencyInjection.cs        # Infrastructure layer DI setup
    │   └── CoherentMobile.Infrastructure.csproj # Infrastructure project file
    │
    └── CoherentMobile.ExternalIntegration/     # 🔌 External Integration Layer
        ├── Clients/
        │   ├── HealthDataApiClient.cs    # External health data API client
        │   └── NotificationApiClient.cs  # Notification service client
        │
        ├── Interfaces/
        │   ├── IHealthDataApiClient.cs   # Health data API interface
        │   └── INotificationApiClient.cs # Notification API interface
        │
        ├── Models/
        │   └── HealthApiModels.cs        # External API models
        │
        ├── DependencyInjection.cs        # External integration DI setup
        └── CoherentMobile.ExternalIntegration.csproj # External integration project file
```

## 📊 Layer Dependencies

```
┌─────────────────────────┐
│    CoherentMobile.API         │  ← Presentation Layer
│  (Controllers, Hubs)    │
└───────────┬─────────────┘
            │
            ├──────────────────────────┐
            │                          │
            ▼                          ▼
┌─────────────────────────┐  ┌─────────────────────────┐
│  CoherentMobile.Application   │  │ CoherentMobile.External       │
│  (Services, Validators) │  │   Integration           │
└───────────┬─────────────┘  └─────────┬───────────────┘
            │                          │
            │                          │
            ▼                          ▼
┌─────────────────────────┐  ┌─────────────────────────┐
│    CoherentMobile.Domain      │◄─┤  CoherentMobile.Infrastructure│
│  (Entities, Interfaces) │  │   (Dapper Repos)        │
└─────────────────────────┘  └─────────────────────────┘
```

## 📝 File Count Summary

| Layer | Files | Purpose |
|-------|-------|---------|
| **Domain** | 7 files | Core entities and interfaces |
| **Application** | 12 files | Business logic and DTOs |
| **Infrastructure** | 6 files | Data access with Dapper |
| **ExternalIntegration** | 6 files | Third-party API clients |
| **API** | 8 files | Controllers, hubs, middleware |
| **Database** | 1 file | SQL schema |
| **Documentation** | 5 files | README, guides, architecture |

**Total**: ~45 files created

## 🎯 Key Files Explained

### Configuration Files

- **`CoherentMobile.sln`**: Visual Studio solution file linking all projects
- **`appsettings.json`**: Main configuration (JWT, CORS, DB, External APIs)
- **`.gitignore`**: Prevents committing build artifacts and secrets

### Entry Point

- **`Program.cs`**: Application startup, DI configuration, middleware pipeline

### Core Business Logic

- **`AuthService.cs`**: Handles user registration, login, JWT token generation
- **`UserService.cs`**: User profile management
- **`HealthRecordService.cs`**: Health data CRUD operations

### Data Access

- **`DapperContext.cs`**: Creates SQL Server connections
- **`UserRepository.cs`**: Dapper queries for User entity
- **`HealthRecordRepository.cs`**: Dapper queries for HealthRecord entity

### API Endpoints

- **`AuthController.cs`**: `/api/auth/register`, `/api/auth/login`
- **`UserController.cs`**: `/api/user/profile`, `/api/user`
- **`HealthRecordController.cs`**: `/api/healthrecord/*`
- **`IntegrationController.cs`**: `/api/integration/*`

### Real-Time Communication

- **`HealthDataHub.cs`**: SignalR hub at `/hubs/healthdata`

### External Integrations

- **`HealthDataApiClient.cs`**: Calls external health data APIs
- **`NotificationApiClient.cs`**: Sends emails, SMS, push notifications

### Middleware

- **`ErrorHandlingMiddleware.cs`**: Catches exceptions globally
- **`RequestLoggingMiddleware.cs`**: Logs all HTTP requests

## 🔧 NuGet Packages by Project

### CoherentMobile.API
```xml
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- Microsoft.AspNetCore.SignalR (1.1.0)
- Serilog.AspNetCore (8.0.0)
- Serilog.Sinks.Console (5.0.1)
- Serilog.Sinks.File (5.0.0)
- Swashbuckle.AspNetCore (6.5.0)
```

### CoherentMobile.Application
```xml
- FluentValidation (11.9.0)
- FluentValidation.DependencyInjectionExtensions (11.9.0)
```

### CoherentMobile.Infrastructure
```xml
- Dapper (2.1.28)
- System.Data.SqlClient (4.8.6)
```

### CoherentMobile.ExternalIntegration
```xml
- Microsoft.Extensions.Http (8.0.0)
- Microsoft.Extensions.Http.Polly (8.0.0)
```

## 🚀 How to Navigate

1. **Start with** `README.md` for overview
2. **Read** `ARCHITECTURE.md` for design details
3. **Follow** `QUICK_START.md` to run the app
4. **Explore** `src/CoherentMobile.Domain` for core entities
5. **Review** `src/CoherentMobile.Application` for business logic
6. **Check** `src/CoherentMobile.API/Controllers` for endpoints
7. **Run** `Database/CreateDatabase.sql` to setup database

## 📌 Important Notes

- **Clean Architecture**: Each layer has specific responsibilities
- **Dependency Rule**: Dependencies point inward (toward Domain)
- **Testability**: Business logic isolated from infrastructure
- **Separation**: External APIs isolated in dedicated layer
- **Security**: JWT authentication throughout
- **Logging**: Serilog structured logging
- **Validation**: Fluent Validation on all inputs
- **Real-time**: SignalR for push notifications

---

**This structure provides a solid foundation for enterprise mobile health applications! 🏥**
