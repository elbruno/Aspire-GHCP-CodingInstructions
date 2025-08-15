using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AspireApp01.Tests;

[TestClass]
public class WebTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Checks if Docker is available and healthy for running distributed tests.
    /// </summary>
    /// <returns>True if Docker is available and healthy, false otherwise.</returns>
    private static bool IsDockerAvailable()
    {
        // Check environment variable override
        var aspireTestDistributed = Environment.GetEnvironmentVariable("ASPIRE_TEST_DISTRIBUTED");
        if (string.Equals(aspireTestDistributed, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            using var process = new Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = "info";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            var timeoutCompleted = process.WaitForExit(5000); // 5 second timeout
            
            if (!timeoutCompleted)
            {
                process.Kill();
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    [TestMethod]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Skip test if Docker is not available
        if (!IsDockerAvailable())
        {
            Assert.Inconclusive("Skipping distributed test because Docker is not available or ASPIRE_TEST_DISTRIBUTED=false. " +
                               "Ensure Docker is running and healthy, or set ASPIRE_TEST_DISTRIBUTED=true to run distributed tests.");
            return;
        }

        // Arrange
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireApp01_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
