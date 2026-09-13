# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first for layer caching
COPY stockDataService.csproj .

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish in Release mode
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime (slim image, no SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser -d /app -s /sbin/nologin appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose the port
EXPOSE 5088

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:5088/openapi/v1.json || exit 1

# Entry point
ENTRYPOINT ["dotnet", "stockDataService.dll"]