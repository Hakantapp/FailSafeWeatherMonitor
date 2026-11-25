using FailSafeWeatherMonitor.ConsoleApp.Data;
using FailSafeWeatherMonitor.ConsoleApp.Entities;
using FailSafeWeatherMonitor.ConsoleApp.Services;
using Spectre.Console;

namespace FailSafeWeatherMonitor.ConsoleApp.UI;

public class MainMenu(
    WeatherFileImporter _weatherFileImporter,
    WeatherApiService _weatherApiService,
    WeatherDataService _weatherDataService,
    string _filePath)
{
    public void RunMenu()
    {
        bool isRunning = true;

        while (isRunning)
        {
            Console.Clear();

            string selectedOption = SelectMenuOption();

            Console.Clear();

            try
            {
                switch (selectedOption)
                {
                    case "Import Weather Data":
                        ImportWeatherFile();
                        break;

                    case "Fetch Live Weather Data":
                        FetchAndSaveWeatherData();
                        break;

                    case "Weather Data Statistics":
                        DisplayWeatherStatistics();
                        break;

                    case "Exit":
                        isRunning = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(
                    $"[red]An error occurred: {Markup.Escape(ex.Message)}[/]");
            }

            if (isRunning)
            {
                ReturnToMenu();
            }
        }
    }

    private static string CenterMenuText(
        string text, int extraWidth = 0)
    {
        int padding = Math.Max(
            0,
            (Console.WindowWidth - text.Length - extraWidth) / 2);

        return new string(' ', padding) + text;
    }
    private void ShowMenuHeader()
    {
        var header = new Panel(
            Align.Center(
                new Markup(
                    "[bold white]FailSafe[/] [bold deepskyblue1]WeatherMonitor[/]\n\n" +
                    "[white]WEATHER MONITORING SYSTEM[/]")))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.DeepSkyBlue1)
            .Padding(2, 1);

        AnsiConsole.Write(Align.Center(header));
        AnsiConsole.WriteLine();

        AnsiConsole.Write(
            new Rule("[deepskyblue1]MAIN MENU[/]")
                .RuleStyle("deepskyblue1"));

        AnsiConsole.WriteLine();
    }

    private void ShowMenuOptions(
     string[] menuOptions, int selectedIndex)
    {
        int optionWidth = menuOptions
            .Max(option => option.Length) + 6;

        for (int i = 0; i < menuOptions.Length; i++)
        {
            string formattedOption = i == selectedIndex
                ? "> " + menuOptions[i]
                : menuOptions[i];

            int padding =
                (optionWidth - formattedOption.Length) / 2;

            formattedOption = formattedOption
                .PadLeft(formattedOption.Length + padding)
                .PadRight(optionWidth);

            if (i == selectedIndex)
            {
                AnsiConsole.Write(
                    Align.Center(
                        new Markup(
                            $"[black on deepskyblue1]{Markup.Escape(formattedOption)}[/]")));
            }
            else
            {
                AnsiConsole.Write(
                    Align.Center(
                        new Markup(
                            $"[white]{Markup.Escape(formattedOption)}[/]")));
            }

            AnsiConsole.WriteLine();
        }
    }
    private string SelectMenuOption()
    {
        string[] menuOptions =
        [
            "Import Weather Data",
            "Fetch Live Weather Data",
            "Weather Data Statistics",
            "Exit"
        ];

        int selectedIndex = 0;

        while (true)
        {
            Console.Clear();

            ShowMenuHeader();

            ShowMenuOptions(menuOptions, selectedIndex);

            AnsiConsole.WriteLine();

            AnsiConsole.Write(
                Align.Center(
                    new Markup(
                        "[grey]Use arrow keys · Enter to select[/]")));

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex =
                        (selectedIndex - 1 + menuOptions.Length)
                        % menuOptions.Length;
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex =
                        (selectedIndex + 1)
                        % menuOptions.Length;
                    break;

                case ConsoleKey.Enter:
                    return menuOptions[selectedIndex];
            }
        }
    }
    private void ReturnToMenu()
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            "[grey]Press any key to return to the main menu.[/]");

        Console.ReadKey(true);
    }

    private void ImportWeatherFile()
    {
        AnsiConsole.Write(
            new Rule("[deepskyblue1]IMPORT WEATHER DATA[/]")
                .RuleStyle("deepskyblue1"));

        AnsiConsole.WriteLine();

        _weatherFileImporter.ImportWeatherDataFromFile(_filePath);
    }

    private void ShowCurrentWeather(WeatherDataPoint weatherDataPoint)
    {
        AnsiConsole.MarkupLine(
            $"[deepskyblue1]Station:[/] {Markup.Escape(weatherDataPoint.StationName)}");

        AnsiConsole.MarkupLine(
            $"[deepskyblue1]Timestamp (UTC):[/] {weatherDataPoint.Timestamp:yyyy-MM-dd HH:mm}");

        AnsiConsole.MarkupLine(
            $"[deepskyblue1]Temperature:[/] {weatherDataPoint.Temperature:F1} °C");

        AnsiConsole.MarkupLine(
            $"[deepskyblue1]Humidity:[/] {weatherDataPoint.Humidity:F1} %");

        AnsiConsole.MarkupLine(
            $"[deepskyblue1]Data source:[/] {Markup.Escape(weatherDataPoint.DataSource)}");
    }

    private void FetchAndSaveWeatherData()
    {
        AnsiConsole.Write(
            new Rule("[deepskyblue1]LIVE WEATHER DATA[/]")
                .RuleStyle("deepskyblue1"));

        AnsiConsole.WriteLine();

        var weatherResponse = _weatherApiService.FetchWeatherResponse();

        if (!_weatherApiService.TryCreateCurrentWeatherDataPoint(
            weatherResponse, out var weatherDataPoint) ||
            weatherDataPoint is null)
        {
            AnsiConsole.MarkupLine(
                "[red]Could not retrieve valid weather data.[/]");

            return;
        }
        ShowCurrentWeather(weatherDataPoint);

        AnsiConsole.WriteLine();

        if (_weatherDataService.IsDuplicate(weatherDataPoint))
        {
            AnsiConsole.MarkupLine(
                "[yellow]This weather measurement already exists.[/]");

            return;
        }

        _weatherDataService.SaveWeatherData([weatherDataPoint]);

        AnsiConsole.MarkupLine(
            "[green]Weather measurement saved successfully.[/]");
    }

    private string? SelectWeatherStation()
    {
        List<string> stations = _weatherDataService.GetUniqueStations();

        if (stations.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]No weather stations found.[/]");

            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[deepskyblue1]Select a weather station:[/]")
                .PageSize(10)
                .WrapAround()
                .HighlightStyle(
                    new Style(Color.Black, Color.DeepSkyBlue1))
                .AddChoices(stations));
    }
    private void ShowMeasurementsTable(
       List<WeatherDataPoint> stationMeasurements)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.DeepSkyBlue1);

        table.AddColumn("[deepskyblue1]Timestamp[/]");
        table.AddColumn("[deepskyblue1]Temp. °C[/]");
        table.AddColumn("[deepskyblue1]Humidity[/]");
        table.AddColumn("[deepskyblue1]Source[/]");

        foreach (var measurement in stationMeasurements)
        {
            table.AddRow(
                measurement.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                $"{measurement.Temperature:F1}",
                $"{measurement.Humidity:F1} %",
                Markup.Escape(measurement.DataSource));
        }

        AnsiConsole.Write(Align.Center(table));
    }

    private void ShowTemperatureSummary(
        List<WeatherDataPoint> stationMeasurements)
    {
        double? maxTemperature =
            _weatherDataService.CalculateMaxTemperature(stationMeasurements);

        double? minTemperature =
            _weatherDataService.CalculateMinTemperature(stationMeasurements);

        double? averageTemperature =
            _weatherDataService.CalculateAverageTemperature(stationMeasurements);

        AnsiConsole.WriteLine();

        AnsiConsole.Write(
            new Rule("[deepskyblue1]TEMPERATURE SUMMARY[/]")
                .RuleStyle("deepskyblue1"));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.DeepSkyBlue1);

        table.AddColumn("[red]Maximum[/]");
        table.AddColumn("[deepskyblue1]Minimum[/]");
        table.AddColumn("[white]Average[/]");

        table.AddRow(
            $"{maxTemperature:F1} °C",
            $"{minTemperature:F1} °C",
            $"{averageTemperature:F1} °C");

        AnsiConsole.Write(Align.Center(table));
    }

    private void DisplayWeatherStatistics()
    {
        AnsiConsole.Write(
            new Rule("[deepskyblue1]WEATHER DATA STATISTICS[/]")
                .RuleStyle("deepskyblue1"));

        AnsiConsole.WriteLine();

        string? stationName = SelectWeatherStation();

        if (stationName is null)
        {
            return;
        }

        List<WeatherDataPoint> stationMeasurements =
            _weatherDataService.GetStationMeasurements(stationName);
        if (stationMeasurements.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]No measurements found for this station.[/]");

            return;
        }

        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold deepskyblue1]Station: {Markup.Escape(stationName)}[/]");

        AnsiConsole.WriteLine();

        ShowMeasurementsTable(stationMeasurements);

        ShowTemperatureSummary(stationMeasurements);
    }
}

