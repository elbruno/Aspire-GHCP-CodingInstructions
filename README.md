# Aspire-GHCP-CodingInstructions
Aspire GitHub CoPilot Coding Instructions

## Running Tests

This project contains both unit tests and distributed/integration tests.

### Unit Tests
Unit tests (like `WeatherApiClientTests` and `ServiceDefaultsExtensionsTests`) do not require any special setup and can be run normally:

```bash
dotnet test AspireApp01/AspireApp01.Tests/AspireApp01.Tests.csproj
```

### Distributed Tests
Distributed tests (like `WebTests.GetWebResourceRootReturnsOkStatusCode`) require Docker to be available and healthy, as they use `Aspire.Hosting.Testing` to start distributed application hosts with container dependencies.

#### Requirements for Distributed Tests:
- Docker Desktop must be installed and running
- Docker daemon must be accessible
- Docker Resource Saver mode should be disabled (if using Docker Desktop)

#### Skipping Distributed Tests:
If Docker is not available, distributed tests will be automatically skipped with an "Inconclusive" result. You can also explicitly skip distributed tests by setting the environment variable:

```bash
ASPIRE_TEST_DISTRIBUTED=false dotnet test
```

#### Troubleshooting:
If you encounter errors like "Container runtime 'docker' was found but appears to be unhealthy", ensure:
1. Docker Desktop is running
2. Docker Resource Saver mode is disabled
3. Docker daemon is accessible (try `docker info` to verify)
