using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FailSafeWeatherMonitor.ConsoleApp.Data;
using FailSafeWeatherMonitor.ConsoleApp.Services;

namespace FailSafeWeatherMonitor.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FailSafe WeatherMonitor startar...");

        var databaseConfiguration = new WeatherDatabaseConfiguration();
        using var dbContext = databaseConfiguration.CreateDbContext(Array.Empty<string>());

        var weatherDataService = new WeatherDataService(dbContext);

        var weatherFileImporter = new WeatherFileImporter(weatherDataService);

        string filePath = Path.Combine(AppContext.BaseDirectory,
            "ImportedFiles",
            "weather_data (1).csv");

        weatherFileImporter.ImportWeatherDataFromFile(filePath);

        var weatherApiService = new WeatherApiService();

        var weatherResponse = weatherApiService.FetchWeatherResponse();

        if (weatherApiService.TryCreateCurrentWeatherDataPoint(
            weatherResponse, out var weatherDataPoint) &&
            weatherDataPoint is not null)
        {
            Console.WriteLine($"Station: {weatherDataPoint.StationName}");
            Console.WriteLine($"Tidstämpel: {weatherDataPoint.Timestamp}");
            Console.WriteLine($"Temperatur: {weatherDataPoint.Temperature} °C");
            Console.WriteLine($"Luftfuktighet: {weatherDataPoint.Humidity} %");
            Console.WriteLine($"Datakälla: {weatherDataPoint.DataSource}");

            if (weatherDataService.IsDuplicate(weatherDataPoint))
            {
                Console.WriteLine("Vädermätningen finns redan i databasen.");
            }
            else
            {
                weatherDataService.SaveWeatherData([weatherDataPoint]);

                Console.WriteLine("Vädermätningen har sparats i databasen.");
            }
        }
        else
        {
            Console.WriteLine("Kunde inte hämta giltig väderdata.");
        }
    }

}    