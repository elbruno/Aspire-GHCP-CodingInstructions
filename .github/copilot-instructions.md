# .NET Aspire Sample Application

This is a .NET Aspire 9.4.1 sample application demonstrating a weather forecast service with a Blazor Server web frontend, API service, Redis caching, and comprehensive testing. The application includes distributed services orchestration, observability with OpenTelemetry, and service discovery.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build (REQUIRED FIRST STEPS)
- Install .NET 9.0 SDK:
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
  export PATH=$HOME/.dotnet:$PATH
  ```
- Navigate to AspireApp01 directory: `cd AspireApp01`
- Restore packages: `dotnet restore` -- takes 51 seconds. NEVER CANCEL. Set timeout to 90+ seconds.
- Build solution: `dotnet build` -- takes 18 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

### Run Tests
- Unit tests (recommended): `dotnet test --filter "FullyQualifiedName~WeatherApiClientTests"` -- takes 4 seconds
- Service tests: `dotnet test --filter "FullyQualifiedName~ServiceDefaultsExtensionsTests"` -- takes 4 seconds  
- All unit tests (10 tests): `dotnet test --filter "FullyQualifiedName!~WebTests"` -- takes 8 seconds
- **DO NOT RUN**: `dotnet test` without filter -- includes 1 integration test that requires full Aspire orchestration setup and will fail in most environments

### Run Individual Services (RECOMMENDED FOR DEVELOPMENT)
- API Service only:
  ```bash
  cd AspireApp01.ApiService
  dotnet run --urls "http://localhost:5000"
  ```
  Test endpoint: `curl http://localhost:5000/weatherforecast`
  
- Web Service only (requires API service running):
  ```bash
  cd AspireApp01.Web  
  dotnet run --urls "http://localhost:5001"
  ```

### Run Full Aspire Application (REQUIRES DOCKER AND ORCHESTRATION)
- **WARNING**: This requires Docker, Kubernetes orchestration, and may not work in sandboxed environments
- Full application: `cd AspireApp01.AppHost && dotnet run`
- Application will attempt to start on https://localhost:17135 or http://localhost:15257
- **EXPECTED FAILURE** in environments without proper orchestration setup

## Validation

### Unit Test Validation (ALWAYS DO THIS)
- Run WeatherApiClient tests: `dotnet test --filter "FullyQualifiedName~WeatherApiClientTests"`
  - 6 tests should pass testing HTTP client functionality, JSON deserialization, error handling
- Run ServiceDefaults tests: `dotnet test --filter "FullyQualifiedName~ServiceDefaultsExtensionsTests"`  
  - 4 tests should pass testing service registration, health checks, OpenTelemetry configuration

### Manual API Validation
- Start API service: `cd AspireApp01.ApiService && dotnet run --urls "http://localhost:5000"`
- Test weather endpoint: `curl http://localhost:5000/weatherforecast`
- Verify JSON response with 5 weather forecasts
- Test health endpoint: `curl http://localhost:5000/health` (in Development environment)

### Build Validation
- Clean build: `dotnet clean && dotnet build`
- Verify no compilation errors
- Check all 5 projects build successfully (ServiceDefaults, ApiService, Web, AppHost, Tests)

## Common Tasks

### Project Structure
```
AspireApp01/
├── AspireApp01.sln              # Solution file
├── AspireApp01.ApiService/      # Weather API service (.NET 9.0)
├── AspireApp01.AppHost/         # Aspire orchestration host (.NET 9.0)  
├── AspireApp01.ServiceDefaults/ # Shared service configuration (.NET 9.0)
├── AspireApp01.Web/             # Blazor Server web app (.NET 9.0)
└── AspireApp01.Tests/           # Unit and integration tests (.NET 9.0)
```

### Key Technologies
- .NET 9.0 (all projects target net9.0)
- .NET Aspire 9.4.1 for distributed application orchestration
- Blazor Server for web UI with interactive components
- Redis for output caching (requires Docker for full orchestration)
- OpenTelemetry for observability and metrics
- xunit and MSTest for testing
- FluentAssertions for test assertions
- MockHttp for HTTP client testing

### Package Management
- Check outdated packages: `dotnet list package --outdated`
- Update specific package: `dotnet add package PackageName -v Version`
- All projects use PackageReference style (no packages.config)
- Current known outdated package: Microsoft.AspNetCore.OpenApi (9.0.2 → 9.0.8) in ApiService project

### Development Workflow
1. Always run `dotnet restore` and `dotnet build` after git pull
2. Run unit tests with filters to avoid integration test failures
3. Test individual services using `dotnet run` in their directories
4. Use the API service at localhost:5000 for testing weather data
5. Make code changes incrementally and test frequently

### Troubleshooting
- If build fails with SDK errors: Ensure .NET 9.0 SDK is installed and in PATH
- If Aspire orchestration fails: This is expected in most environments; use individual service testing
- If Redis integration tests fail: This is expected without Docker/Redis setup
- If packages restore slowly: This is normal; wait for completion (up to 90 seconds)

### Files to Check After Changes
- Always run unit tests after changes to WeatherApiClient.cs
- Always run service tests after changes to ServiceDefaults/Extensions.cs
- Always test API endpoint after changes to AspireApp01.ApiService/Program.cs
- Check build after changes to any .csproj files

## Timing Expectations (CRITICAL - NEVER CANCEL)

### Build Times
- `dotnet restore`: 51 seconds. NEVER CANCEL. Set timeout to 90+ seconds.
- `dotnet build`: 18 seconds. NEVER CANCEL. Set timeout to 60+ seconds.  
- `dotnet clean && dotnet build`: 20 seconds. NEVER CANCEL. Set timeout to 60+ seconds.

### Test Times  
- Unit tests (filtered): 4-8 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
- All tests (including failing integration): 25+ seconds. NEVER CANCEL. Set timeout to 60+ seconds.

### Service Startup
- Individual service startup: 2-5 seconds
- API service responds within 1 second after startup
- Full Aspire orchestration: May timeout (20+ seconds) in unsupported environments

**CRITICAL**: Always use adequate timeouts. Build and test operations can take longer than expected, especially on slower systems or during initial package downloads.

## Quick Reference Commands

### Common Outputs for Reference (save time over running commands)

#### Repository structure
```
AspireApp01/
├── AspireApp01.sln
├── AspireApp01.ApiService/
│   ├── AspireApp01.ApiService.csproj
│   ├── Program.cs
│   └── Properties/
├── AspireApp01.AppHost/
│   ├── AspireApp01.AppHost.csproj
│   ├── AppHost.cs
│   └── Properties/
├── AspireApp01.ServiceDefaults/
│   ├── AspireApp01.ServiceDefaults.csproj
│   └── Extensions.cs
├── AspireApp01.Web/
│   ├── AspireApp01.Web.csproj
│   ├── Program.cs
│   ├── WeatherApiClient.cs
│   └── Components/
└── AspireApp01.Tests/
    ├── AspireApp01.Tests.csproj
    ├── ServiceDefaultsExtensionsTests.cs
    ├── WeatherApiClientTests.cs
    └── WebTests.cs
```

#### Example API Response
```json
[
  {"date":"2025-08-16","temperatureC":-5,"summary":"Warm","temperatureF":24},
  {"date":"2025-08-17","temperatureC":39,"summary":"Chilly","temperatureF":102},
  {"date":"2025-08-18","temperatureC":24,"summary":"Balmy","temperatureF":75},
  {"date":"2025-08-19","temperatureC":-11,"summary":"Chilly","temperatureF":13},
  {"date":"2025-08-20","temperatureC":18,"summary":"Scorching","temperatureF":64}
]
```