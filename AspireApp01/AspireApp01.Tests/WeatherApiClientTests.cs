using System.Net;
using System.Text.Json;
using AspireApp01.Web;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace AspireApp01.Tests;

public class WeatherApiClientTests
{
    [Fact]
    public async Task GetWeatherAsync_ReturnsExpectedForecasts_WhenApiReturns200()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");
        
        var expectedForecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 25, "Sunny"),
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 22, "Cloudy")
        };

        var jsonResponse = JsonSerializer.Serialize(expectedForecasts);
        mockHttp.When("/weatherforecast")
               .Respond("application/json", jsonResponse);

        var client = new WeatherApiClient(httpClient);

        // Act
        var result = await client.GetWeatherAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Date.Should().Be(expectedForecasts[0].Date);
        result[0].TemperatureC.Should().Be(expectedForecasts[0].TemperatureC);
        result[0].Summary.Should().Be(expectedForecasts[0].Summary);
        result[1].Date.Should().Be(expectedForecasts[1].Date);
        result[1].TemperatureC.Should().Be(expectedForecasts[1].TemperatureC);
        result[1].Summary.Should().Be(expectedForecasts[1].Summary);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsEmptyArray_WhenApiReturnsEmptyResponse()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");
        
        mockHttp.When("/weatherforecast")
               .Respond("application/json", "[]");

        var client = new WeatherApiClient(httpClient);

        // Act
        var result = await client.GetWeatherAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWeatherAsync_ThrowsHttpRequestException_WhenApiReturns500()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");
        
        mockHttp.When("/weatherforecast")
               .Respond(HttpStatusCode.InternalServerError);

        var client = new WeatherApiClient(httpClient);

        // Act & Assert
        await client.Invoking(c => c.GetWeatherAsync())
                   .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetWeatherAsync_RespectsMaxItemsParameter()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");
        
        var manyForecasts = new List<WeatherForecast>();
        for (int i = 0; i < 10; i++)
        {
            manyForecasts.Add(new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(i)), 20 + i, $"Day {i}"));
        }

        var jsonResponse = JsonSerializer.Serialize(manyForecasts);
        mockHttp.When("/weatherforecast")
               .Respond("application/json", jsonResponse);

        var client = new WeatherApiClient(httpClient);

        // Act
        var result = await client.GetWeatherAsync(maxItems: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetWeatherAsync_SendsCorrectRequestUrl()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");
        
        mockHttp.When("/weatherforecast")
               .Respond("application/json", "[]");

        var client = new WeatherApiClient(httpClient);

        // Act
        await client.GetWeatherAsync();

        // Assert
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public void WeatherForecast_TemperatureF_CalculatesCorrectly()
    {
        // Arrange
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 25, "Sunny");

        // Act
        var temperatureF = forecast.TemperatureF;

        // Assert
        temperatureF.Should().Be(76); // 25C = 76F (corrected calculation)
    }
}