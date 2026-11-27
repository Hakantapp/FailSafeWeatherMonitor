using FailSafeWeatherMonitor.ConsoleApp.Entities;

namespace FailSafeWeatherMonitor.ConsoleApp.Data;

public class WeatherDataService(WeatherDbContext _db)
{
    public bool IsDuplicate(
        WeatherDataPoint weatherDataPoint,
        List<WeatherDataPoint>? validWeatherDataPoints = null)
    {
        return _db.WeatherDataPoints.Any(w =>
            w.StationName == weatherDataPoint.StationName &&
            w.Timestamp == weatherDataPoint.Timestamp)
            ||
            (validWeatherDataPoints?.Any(w =>
                w.StationName == weatherDataPoint.StationName &&
                w.Timestamp == weatherDataPoint.Timestamp) ?? false);
    }

    public void SaveWeatherData(
        List<WeatherDataPoint> validWeatherDataPoints)
    {
        _db.WeatherDataPoints.AddRange(validWeatherDataPoints);
        _db.SaveChanges();
    }

    public List<string> GetUniqueStations()
    {
        return _db.WeatherDataPoints
            .Select(w => w.StationName)
            .Distinct()
            .OrderBy(w => w)
            .ToList();
    }

    public List<WeatherDataPoint> GetStationMeasurements(string stationName)
    {
        return _db.WeatherDataPoints
            .Where(w => w.StationName == stationName)
            .OrderBy(w => w.Timestamp)
            .ToList();
    }

    public double? CalculateMaxTemperature(
    List<WeatherDataPoint> stationMeasurements)
    {
        return stationMeasurements.Max(w => (double?)w.Temperature);
    }

    public double? CalculateMinTemperature(
        List<WeatherDataPoint> stationMeasurements)
    {
        return stationMeasurements.Min(w => (double?)w.Temperature);
    }

    public double? CalculateAverageTemperature(
        List<WeatherDataPoint> stationMeasurements)
    {
        return stationMeasurements.Average(w => (double?)w.Temperature);
    }
}