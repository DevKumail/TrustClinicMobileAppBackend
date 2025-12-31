# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY CoherentMobile.Domain/CoherentMobile.Domain.csproj CoherentMobile.Domain/
COPY CoherentMobile.Application/CoherentMobile.Application.csproj CoherentMobile.Application/
COPY CoherentMobile.Infrastructure/CoherentMobile.Infrastructure.csproj CoherentMobile.Infrastructure/
COPY CoherentMobile.ExternalIntegration/CoherentMobile.ExternalIntegration.csproj CoherentMobile.ExternalIntegration/
COPY CoherentMobile.Api/CoherentMobile.Api.csproj CoherentMobile.Api/

RUN dotnet restore CoherentMobile.Api/CoherentMobile.Api.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish CoherentMobile.Api/CoherentMobile.Api.csproj -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

# Expose port (Render sets PORT env variable)
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "CoherentMobile.Api.dll"]
