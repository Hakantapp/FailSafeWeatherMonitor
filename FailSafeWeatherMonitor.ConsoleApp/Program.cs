using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FailSafeWeatherMonitor.ConsoleApp.Data;

namespace FailSafeWeatherMonitor.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FailSafe WeatherMonitor startar...");

        var databaseConfiguration = new WeatherDatabaseConfiguration();
        using var dbContext = databaseConfiguration.CreateDbContext(Array.Empty<string>());
    }
}

       