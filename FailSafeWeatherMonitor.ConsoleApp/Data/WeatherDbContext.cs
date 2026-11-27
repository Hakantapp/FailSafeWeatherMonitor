using FailSafeWeatherMonitor.ConsoleApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace FailSafeWeatherMonitor.ConsoleApp.Data;

public class WeatherDbContext : DbContext
{

    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }
    public DbSet<WeatherDataPoint> WeatherDataPoints { get; set; } 


}
