# Coherent Mobile Health - Architecture Documentation

## 🏛️ Clean Architecture Overview

This project follows **Clean Architecture** (also known as Onion Architecture) to achieve:
- **Separation of Concerns**: Each layer has specific responsibilities
- **Dependency Inversion**: Dependencies point inward toward the domain
- **Testability**: Business logic isolated from infrastructure
- **Maintainability**: Changes in one layer don't affect others
- **Technology Independence**: Easy to swap frameworks and tools

## 📐 Layer Breakdown

### 1. Domain Layer (`CoherentMobile.Domain`)

**Purpose**: Core business entities and repository interfaces

**Responsibilities**:
- Define domain entities (User, HealthRecord)
- Define repository interfaces (IUserRepository, IHealthRecordRepository)
- Define domain-specific interfaces (IUnitOfWork)
- No dependencies on other layers

**Key Principles**:
- Pure domain logic
- No external dependencies
- No framework references
- Technology-agnostic

**Example Entities**:
```csharp
// Domain/Entities/User.cs
public class User : BaseEntity
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    // ... other properties
}
```

### 2. Application Layer (`CoherentMobile.Application`)

**Purpose**: Business logic and use cases

**Responsibilities**:
- Define DTOs (Data Transfer Objects)
- Implement service interfaces
- Define business workflows
- Input validation with Fluent Validation
- Orchestrate domain operations

**Dependencies**:
- Domain Layer only

**Key Components**:
- **Services**: AuthService, UserService, HealthRecordService
- **DTOs**: RegisterUserDto, LoginDto, HealthRecordDto
- **Validators**: RegisterUserValidator, LoginValidator
- **Interfaces**: IAuthService, IUserService

**Example Service**:
```csharp
// Application/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return MapToDto(user);
    }
}
```

### 3. Infrastructure Layer (`CoherentMobile.Infrastructure`)

**Purpose**: Data access and external services implementation

**Responsibilities**:
- Implement repository interfaces using Dapper
- Database connection management
- Data persistence logic
- Transaction management (Unit of Work)

**Dependencies**:
- Domain Layer
- Application Layer (for some interface implementations)

**Key Components**:
- **DapperContext**: Database connection factory
- **Repositories**: UserRepository, HealthRecordRepository
- **UnitOfWork**: Transaction coordination

**Example Repository**:
```csharp
// Infrastructure/Repositories/UserRepository.cs
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        var query = "SELECT * FROM Users WHERE Email = @Email";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
    }
}
```

### 4. External Integration Layer (`CoherentMobile.ExternalIntegration`)

**Purpose**: Isolated third-party API integration

**Responsibilities**:
- Call external REST APIs
- Handle HTTP communication
- Implement retry policies
- Map external models to internal DTOs

**Dependencies**:
- Domain Layer only (for domain models if needed)

**Key Components**:
- **HealthDataApiClient**: Integration with health data providers
- **NotificationApiClient**: Email, SMS, push notification services
- **HTTP Client Factory**: Configured with Polly for resilience

**Why Separate Layer?**:
- Keeps third-party dependencies isolated
- Easy to mock for testing
- Clear boundary for external systems
- Can be replaced without affecting business logic

**Example Client**:
```csharp
// ExternalIntegration/Clients/HealthDataApiClient.cs
public class HealthDataApiClient : IHealthDataApiClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<ExternalHealthDataResponse?> GetHealthDataAsync(string userId, string dataType)
    {
        var response = await _httpClient.GetAsync($"/api/health/{userId}/{dataType}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExternalHealthDataResponse>();
    }
}
```

### 5. API Layer (`CoherentMobile.API`)

**Purpose**: Presentation layer exposing HTTP endpoints

**Responsibilities**:
- REST API controllers
- SignalR hubs for real-time communication
- Middleware (error handling, logging, authentication)
- Request/response formatting
- API documentation (Swagger)

**Dependencies**:
- Application Layer
- Infrastructure Layer
- External Integration Layer

**Key Components**:
- **Controllers**: AuthController, UserController, HealthRecordController
- **Hubs**: HealthDataHub (SignalR)
- **Middleware**: ErrorHandlingMiddleware, RequestLoggingMiddleware
- **Program.cs**: Dependency injection and app configuration

**Example Controller**:
```csharp
// API/Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);
        return Ok(response);
    }
}
```

## 🔄 Dependency Flow

```
┌─────────────────────────────────────┐
│         CoherentMobile.API                │
│    (Controllers, Hubs, Middleware)  │
└─────────────┬───────────────────────┘
              │ depends on
              ▼
┌─────────────────────────────────────┐
│      CoherentMobile.Application           │
│   (Services, DTOs, Validators)      │
└─────────────┬───────────────────────┘
              │ depends on
              ▼
┌─────────────────────────────────────┐
│       CoherentMobile.Domain               │
│  (Entities, Interfaces) - Core      │
└─────────────────────────────────────┘
              ▲
              │ implements
              │
┌─────────────┴───────────────────────┐
│   CoherentMobile.Infrastructure           │
│  (Dapper Repositories, Data Access) │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│  CoherentMobile.ExternalIntegration       │
│   (Third-Party API Clients)         │
└─────────────────────────────────────┘
```

## 🎯 Cross-Cutting Concerns

### 1. Authentication & Authorization
- **JWT Token Authentication** configured in `Program.cs`
- Token generation in `AuthService`
- `[Authorize]` attribute on protected endpoints

### 2. Logging
- **Serilog** configured for structured logging
- Logs to console and rolling file
- Request/response logging via middleware

### 3. Validation
- **Fluent Validation** for DTO validation
- Validators registered in Application layer
- Automatic validation in controllers

### 4. Error Handling
- Global exception middleware catches all errors
- Returns standardized error responses
- Logs exceptions with Serilog

### 5. CORS
- Configured in `Program.cs`
- Supports credentials for SignalR
- Configurable allowed origins

## 🔌 Integration Points

### Database (Dapper)
- **Connection**: Configured in `appsettings.json`
- **Context**: `DapperContext` creates SQL connections
- **Repositories**: Execute raw SQL queries with Dapper

### SignalR (Real-Time)
- **Hub**: `HealthDataHub` at `/hubs/healthdata`
- **Authentication**: JWT token via query string
- **Groups**: User-specific groups for targeted messaging

### External APIs
- **HTTP Clients**: Configured with Polly retry policies
- **Typed Clients**: Interface-based for testability
- **Configuration**: API keys and base URLs in `appsettings.json`

## 🧩 Design Patterns Used

1. **Repository Pattern**: Data access abstraction
2. **Unit of Work**: Transaction management
3. **Dependency Injection**: Loose coupling
4. **Factory Pattern**: Connection creation
5. **Strategy Pattern**: Validation strategies
6. **Adapter Pattern**: External API integration
7. **Middleware Pattern**: Request pipeline
8. **Observer Pattern**: SignalR real-time updates

## 📦 Package Organization

Each layer has its own `.csproj` file with specific dependencies:

- **Domain**: No external dependencies
- **Application**: FluentValidation, Configuration abstractions
- **Infrastructure**: Dapper, SQL Client
- **ExternalIntegration**: HttpClient, Polly
- **API**: ASP.NET Core, JWT, SignalR, Serilog, Swagger

## 🚀 Benefits of This Architecture

1. **Testability**: Easy to unit test business logic
2. **Maintainability**: Clear separation makes changes easier
3. **Scalability**: Can add features without breaking existing code
4. **Flexibility**: Easy to swap out infrastructure components
5. **Team Collaboration**: Different teams can work on different layers
6. **Technology Independence**: Core business logic doesn't depend on frameworks

## 🔧 Extension Points

To add new features:

1. **New Entity**: Add to Domain layer
2. **New Repository**: Implement interface in Infrastructure
3. **New Service**: Add to Application layer
4. **New Endpoint**: Create controller in API layer
5. **New External API**: Add client to ExternalIntegration layer

## 📝 Best Practices

- Keep domain layer pure and framework-agnostic
- Use interfaces for all external dependencies
- Validate input at the API boundary
- Handle errors gracefully with middleware
- Log important operations
- Use async/await throughout
- Follow SOLID principles
- Write self-documenting code

---

This architecture provides a solid foundation for building scalable, maintainable enterprise applications.
