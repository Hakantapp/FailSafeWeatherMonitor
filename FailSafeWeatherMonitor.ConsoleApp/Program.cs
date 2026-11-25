
using FailSafeWeatherMonitor.ConsoleApp.Data;
using FailSafeWeatherMonitor.ConsoleApp.Services;
using FailSafeWeatherMonitor.ConsoleApp.UI;
using System.Text;

namespace FailSafeWeatherMonitor.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var databaseConfiguration = new WeatherDatabaseConfiguration();

        using var dbContext =
            databaseConfiguration.CreateDbContext(Array.Empty<string>());

        var weatherDataService = new WeatherDataService(dbContext);

        var weatherFileImporter = new WeatherFileImporter(weatherDataService);

        var weatherApiService = new WeatherApiService();

        string filePath = Path.Combine(
            AppContext.BaseDirectory,
            "ImportedFiles",
            "weather_data (1).csv");

        var mainMenu = new MainMenu(
            weatherFileImporter,
            weatherApiService,
            weatherDataService,
            filePath);

        mainMenu.RunMenu();
    }
}
