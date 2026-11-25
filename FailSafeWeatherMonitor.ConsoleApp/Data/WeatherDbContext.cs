using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FailSafeWeatherMonitor.ConsoleApp.Entities;

namespace FailSafeWeatherMonitor.ConsoleApp.Data;

public class WeatherDbContext : DbContext
{

    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }
    public DbSet<WeatherDataPoint> WeatherDataPoints { get; set; } 


}
