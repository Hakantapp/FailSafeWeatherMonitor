using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Globalization;
using FailSafeWeatherMonitor.ConsoleApp.Entities;
using System.Linq.Expressions;
using FailSafeWeatherMonitor.ConsoleApp.Services;

namespace FailSafeWeatherMonitor.ConsoleApp.Data;

public class WeatherFileImporter(WeatherDbContext _db)
{
    public void ImportWeatherDataFromFile(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            var errors = new List<string>();
            var validWeatherDataPoints = new List<WeatherDataPoint>();
            int duplicateCount = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                if (!TryCreateWeatherDataPoint(lines[i],
                    out WeatherDataPoint? weatherDataPoint))
                {
                    errors.Add(lines[i]);
                    continue;
                }

                if (weatherDataPoint is not null)
                {
                    if (IsDuplicate(weatherDataPoint, validWeatherDataPoints))
                    {
                        duplicateCount++;
                        continue;
                    }

                    validWeatherDataPoints.Add(weatherDataPoint);
                }
            }

            WriteErrorLog(errors);
            SaveWeatherData(validWeatherDataPoints);

            Console.WriteLine($"Importerade mätningar: {validWeatherDataPoints.Count}");
            Console.WriteLine($"Förkastade mätningar: {errors.Count}");
            Console.WriteLine($"Överhoppade dubbletter: {duplicateCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid import av väderdata: {ex.Message}");
        }
    }


    private bool TryCreateWeatherDataPoint(string line,
    out WeatherDataPoint? weatherDataPoint)
    {
        weatherDataPoint = null;

        var parts = line.Split('\t');

        if (parts.Length != 4)
        {
            return false;
        }

        string stationName = parts[0].Trim();
        string timestampRaw = parts[1].Trim();
        string temperatureRaw = parts[2].Trim();
        string humidityRaw = parts[3].Trim();

        if (string.IsNullOrWhiteSpace(stationName))
        {
            return false;
        }

        if (!DateTime.TryParse(timestampRaw, out DateTime timestamp))
        {
            return false;
        }

        if (!double.TryParse(temperatureRaw, NumberStyles.Float,
            CultureInfo.InvariantCulture, out double temperature))
        {
            return false;
        }

        if (!WeatherDataValidator.IsValidTemperature(temperature))
        {
            return false;
        }

        if (!double.TryParse(humidityRaw, NumberStyles.Float,
            CultureInfo.InvariantCulture, out double humidity))
        {
            return false;
        }

        if (!WeatherDataValidator.IsValidHumidity(humidity))
        {
            return false;
        }

        weatherDataPoint = new WeatherDataPoint
        {
            StationName = stationName,
            Timestamp = timestamp,
            Temperature = temperature,
            Humidity = humidity,
            DataSource = "CSV-File"
        };

        return true;
    }

    private void WriteErrorLog(List<string> errors)
    {
        File.WriteAllLines("bad_data.log", errors);
    }

    private bool IsDuplicate(WeatherDataPoint weatherDataPoint,
        List<WeatherDataPoint> validWeatherDataPoints)
    {
        return _db.WeatherDataPoints.Any(w =>
            w.StationName == weatherDataPoint.StationName &&
            w.Timestamp == weatherDataPoint.Timestamp)
            ||
            validWeatherDataPoints.Any(w =>
            w.StationName == weatherDataPoint.StationName &&
            w.Timestamp == weatherDataPoint.Timestamp);
    }

    private void SaveWeatherData(List<WeatherDataPoint> validWeatherDataPoints)
    {
        _db.WeatherDataPoints.AddRange(validWeatherDataPoints);
        _db.SaveChanges();
    }
}
