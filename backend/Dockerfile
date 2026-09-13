# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching (restore dependencies separately)
COPY Infrastructure/Backend.Infrastructure.csproj Infrastructure/
COPY Application/Backend.Application.csproj Application/
COPY Domain/Backend.Domain.csproj Domain/

# Restore dependencies (cached unless csproj files change)
RUN dotnet restore Infrastructure/Backend.Infrastructure.csproj

# Copy all source code
COPY . .

# Build and publish in Release mode
WORKDIR /src/Infrastructure
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime (slim image, no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser -d /app -s /sbin/nologin appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose the port
EXPOSE 5195

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:5195/api/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Backend.Infrastructure.dll"]