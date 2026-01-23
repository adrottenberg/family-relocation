# Multi-stage build for FamilyRelocation API
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY FamilyRelocation.sln ./
COPY src/FamilyRelocation.API/FamilyRelocation.API.csproj src/FamilyRelocation.API/
COPY src/FamilyRelocation.Application/FamilyRelocation.Application.csproj src/FamilyRelocation.Application/
COPY src/FamilyRelocation.Domain/FamilyRelocation.Domain.csproj src/FamilyRelocation.Domain/
COPY src/FamilyRelocation.Infrastructure/FamilyRelocation.Infrastructure.csproj src/FamilyRelocation.Infrastructure/
COPY tests/FamilyRelocation.Application.Tests/FamilyRelocation.Application.Tests.csproj tests/FamilyRelocation.Application.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/
COPY tests/ tests/

# Build and publish
RUN dotnet publish src/FamilyRelocation.API/FamilyRelocation.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FamilyRelocation.API.dll"]
