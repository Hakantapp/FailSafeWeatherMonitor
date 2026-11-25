using FailSafeWeatherMonitor.ConsoleApp.Entities;
using System.Globalization;
using System.Text.Json;

namespace FailSafeWeatherMonitor.ConsoleApp.Services;

public class WeatherApiService
{
    private static readonly HttpClient _httpClient = new();

    private const string WeatherApiUrl =
        "https://api.open-meteo.com/v1/forecast?" +
        "latitude=57.6832&longitude=11.9511&" +
        "hourly=temperature_2m,relative_humidity_2m";

    public WeatherResponse? FetchWeatherResponse()
    {
        try
        {
            string weatherJson = _httpClient
                .GetStringAsync(WeatherApiUrl)
                .GetAwaiter()
                .GetResult();

            return JsonSerializer.Deserialize<WeatherResponse>(weatherJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid hämtning av väderdata: {ex.Message}");
            return null;
        }
    }

    public bool TryCreateCurrentWeatherDataPoint(
        WeatherResponse? weatherResponse,
        out WeatherDataPoint? weatherDataPoint)
    {
        weatherDataPoint = null;

        if (weatherResponse?.Hourly is null ||
            !HasValidWeatherData(weatherResponse.Hourly))
        {
            return false;
        }

        if (!TryFindCurrentWeatherIndex(
            weatherResponse.Hourly,
            out int currentIndex,
            out DateTime timestamp))
        {
            return false;
        }

        if (!TryGetWeatherValues(
            weatherResponse.Hourly,
            currentIndex,
            out double temperature,
            out double humidity))
        {
            return false;
        }

        weatherDataPoint = new WeatherDataPoint
        {
            StationName = "Göteborg",
            Timestamp = timestamp,
            Temperature = temperature,
            Humidity = humidity,
            DataSource = "API"
        };

        return true;
    }

    private bool HasValidWeatherData(HourlyWeatherData hourly)
    {
        return hourly.Timestamps is not null &&
            hourly.Temperatures is not null &&
            hourly.HumidityValues is not null &&
            hourly.Timestamps.Length > 0 &&
            hourly.Timestamps.Length == hourly.Temperatures.Length &&
            hourly.Timestamps.Length == hourly.HumidityValues.Length;
    }

    private bool TryFindCurrentWeatherIndex(
        HourlyWeatherData hourly,
        out int currentIndex,
        out DateTime timestamp)
    {
        DateTime currentTime = DateTime.UtcNow;

        currentIndex = -1;
        timestamp = default;

        for (int i = hourly.Timestamps.Length - 1; i >= 0; i--)
        {
            if (DateTime.TryParse(
                hourly.Timestamps[i],
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal,
                out timestamp))
            {
                if (timestamp <= currentTime)
                {
                    currentIndex = i;
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryGetWeatherValues(
        HourlyWeatherData hourly,
        int currentIndex,
        out double temperature,
        out double humidity)
    {
        temperature = default;
        humidity = default;

        if (hourly.Temperatures[currentIndex] is not double temperatureValue ||
            hourly.HumidityValues[currentIndex] is not double humidityValue)
        {
            return false;
        }

        temperature = temperatureValue;
        humidity = humidityValue;

        return WeatherDataValidator.IsValidTemperature(temperature) &&
            WeatherDataValidator.IsValidHumidity(humidity);
    }
}