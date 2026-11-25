using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json.Serialization;

namespace FailSafeWeatherMonitor.ConsoleApp.Services;

public class WeatherResponse
{
    [JsonPropertyName("hourly")]
    public HourlyWeatherData? Hourly { get; set; }
}

public class HourlyWeatherData
{
    [JsonPropertyName("time")]
    public string[] Timestamps { get; set; } = [];

    [JsonPropertyName("temperature_2m")]
    public double?[] Temperatures { get; set; } = [];

    [JsonPropertyName("relative_humidity_2m")]
    public double?[] HumidityValues { get; set; } = [];
}