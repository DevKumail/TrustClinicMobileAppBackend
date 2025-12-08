using System.Text;
using CoherentMobile.API.Hubs;
using CoherentMobile.API.Middleware;
using CoherentMobile.Application;
using CoherentMobile.ExternalIntegration;
using CoherentMobile.Infrastructure;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog Configuration =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/coherent-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ===== Add services to the container =====

// Add Controllers with Fluent Validation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// Add Layer Dependencies
builder.Services.AddApplicationServices();      // Application layer
builder.Services.AddInfrastructureServices();   // Infrastructure layer
builder.Services.AddExternalIntegrationServices(builder.Configuration); // External integration layer

// ===== JWT Authentication Configuration =====
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? "YourSuperSecretKeyMinimum32Characters!!";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ===== CORS Configuration =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("CoherentCorsPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000", "https://localhost:3000" }
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});

// ===== SignalR Configuration =====
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ===== Swagger/OpenAPI Configuration =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Coherent Mobile Health API",
        Version = "v1",
        Description = "Enterprise-grade mobile health application API with Clean Architecture",
        Contact = new OpenApiContact
        {
            Name = "Coherent Health Team",
            Email = "support@coherenthealth.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== Health Checks =====
builder.Services.AddHealthChecks();

// ===== Build the application =====
var app = builder.Build();

// ===== Configure the HTTP request pipeline =====

// Global Error Handling Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Request Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// Enable Swagger in all environments (you can restrict to Development if needed)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coherent API V1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseHttpsRedirection();

// Enable static files for images
app.UseStaticFiles();

// CORS must be before Authentication
app.UseCors("CoherentCorsPolicy");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<HealthDataHub>("/hubs/healthdata");

// Health Check Endpoint
app.MapHealthChecks("/health");

// Welcome endpoint
app.MapGet("/", () => new
{
    Application = "Coherent Mobile Health API",
    Version = "1.0.0",
    Status = "Running",
    Documentation = "Navigate to /swagger for API documentation",
    SwaggerUrl = "/swagger",
    Timestamp = DateTime.UtcNow
});

Log.Information("Starting Coherent Mobile Health API");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
