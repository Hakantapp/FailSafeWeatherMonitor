using System;
using System.Collections.Generic;
using System.Text;


namespace FailSafeWeatherMonitor.ConsoleApp.Services;




public static class WeatherDataValidator
{
    public static bool IsValidTemperature(double temperature)
    {
        return double.IsFinite(temperature) &&
            temperature >= -60 && temperature <= 60;
    }

    public static bool IsValidHumidity(double humidity)
    {
        return double.IsFinite(humidity) &&
            humidity >= 0 && humidity <= 100;
    }
}