using FailSafeWeatherMonitor.ConsoleApp.Entities;
using FailSafeWeatherMonitor.ConsoleApp.Services;
using System.Globalization;
namespace FailSafeWeatherMonitor.ConsoleApp.Data;

public class WeatherFileImporter(WeatherDataService _weatherDataService)
{
    public (int ImportedCount, int RejectedCount, int DuplicateCount)
        ImportWeatherDataFromFile(string filePath)
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
                if (_weatherDataService.IsDuplicate(
                    weatherDataPoint, validWeatherDataPoints))
                {
                    duplicateCount++;
                    continue;
                }

                validWeatherDataPoints.Add(weatherDataPoint);
            }
        }

        WriteErrorLog(errors);
        _weatherDataService.SaveWeatherData(validWeatherDataPoints);

        return (validWeatherDataPoints.Count, errors.Count, duplicateCount);
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
}
