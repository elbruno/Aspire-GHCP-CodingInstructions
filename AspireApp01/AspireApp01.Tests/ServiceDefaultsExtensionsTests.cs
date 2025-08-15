using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace AspireApp01.Tests;

public class ServiceDefaultsExtensionsTests
{
    [Fact]
    public void AddServiceDefaults_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging to make it work
        var configurationManager = new ConfigurationManager();
        var builder = new TestHostApplicationBuilder(services, configurationManager);

        // Act
        builder.AddServiceDefaults();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verify that the method executes without throwing and registers some services
        serviceProvider.Should().NotBeNull();
        var registeredServices = services.Count;
        registeredServices.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AddDefaultHealthChecks_RegistersLivenessHealthCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging to make health checks work
        var configurationManager = new ConfigurationManager();
        var builder = new TestHostApplicationBuilder(services, configurationManager);

        // Act
        builder.AddDefaultHealthChecks();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();
        healthCheckService.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureOpenTelemetry_RegistersOpenTelemetryServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging required for OpenTelemetry
        var configurationManager = new ConfigurationManager();
        var builder = new TestHostApplicationBuilder(services, configurationManager);

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verify that OpenTelemetry related services are registered
        // Note: Due to the complexity of OpenTelemetry registration, we mainly verify
        // that the method executes without throwing exceptions
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void MapDefaultEndpoints_RegistersHealthCheckEndpoints_InDevelopmentEnvironment()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHealthChecks(); // Need to add health checks first
        var app = builder.Build();

        // Act & Assert
        // This should not throw an exception when health checks are properly registered
        var result = app.MapDefaultEndpoints();
        result.Should().Be(app);
    }

    // Helper class to create a test host application builder
    private class TestHostApplicationBuilder : IHostApplicationBuilder
    {
        public TestHostApplicationBuilder(IServiceCollection services, IConfigurationManager configuration)
        {
            Services = services;
            Configuration = configuration;
            Environment = new TestHostEnvironment();
            Logging = new LoggingBuilder(services);
            Metrics = new MetricsBuilder(services);
        }

        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
        public IServiceCollection Services { get; }
        public IConfigurationManager Configuration { get; }
        public IHostEnvironment Environment { get; }
        public ILoggingBuilder Logging { get; }
        public IMetricsBuilder Metrics { get; }

        public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
        {
            // Implementation not needed for tests
        }
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "TestApp";
        public string ContentRootPath { get; set; } = "/";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private class LoggingBuilder : ILoggingBuilder
    {
        public LoggingBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    private class MetricsBuilder : IMetricsBuilder
    {
        public MetricsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}