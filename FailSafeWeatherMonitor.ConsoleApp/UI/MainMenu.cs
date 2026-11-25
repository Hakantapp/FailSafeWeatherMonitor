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
                        if (DisplayWeatherStatistics())
                        {
                            continue;
                        }
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

    private void ShowMenuHeader(string title, bool showHeader)
    {
        if (showHeader)
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
        }

        AnsiConsole.Write(
            new Rule($"[deepskyblue1]{Markup.Escape(title)}[/]")
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

        return SelectOption(menuOptions, "MAIN MENU", true);
    }

    private string SelectOption(
        string[] menuOptions, string title, bool showHeader)
    {
        int selectedIndex = 0;

        while (true)
        {
            Console.Clear();

            ShowMenuHeader(title, showHeader);

            if (!showHeader)
            {
                AnsiConsole.Write(Align.Center(
                    new Markup("[deepskyblue1]Select a weather station[/]")));
                AnsiConsole.WriteLine();
            }

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
        AnsiConsole.WriteLine();

        AnsiConsole.Write(Align.Center(new Markup(
            "[grey]Press any key to return to the main menu.[/]")));
        AnsiConsole.WriteLine();

        Console.ReadKey(true);
    }

    private void ImportWeatherFile()
    {
        AnsiConsole.Write(
            new Rule("[deepskyblue1]IMPORT WEATHER DATA[/]")
                .RuleStyle("deepskyblue1"));

        AnsiConsole.WriteLine();

        var importResult = _weatherFileImporter.ImportWeatherDataFromFile(_filePath);

        ShowImportSummaryPanels(importResult);
    }

    private void ShowImportSummaryPanels(
        (int ImportedCount, int RejectedCount, int DuplicateCount) importResult)
    {
        var table = new Table()
            .NoBorder()
            .HideHeaders();

        table.AddColumn(new TableColumn(""));
        table.AddColumn(new TableColumn(""));
        table.AddColumn(new TableColumn(""));

        table.AddRow(
            new Panel(Align.Center(new Markup(
                $"[green]Imported[/]\n\n[bold green]{importResult.ImportedCount}[/]\nMeasurements")))
                .Border(BoxBorder.Rounded).BorderColor(Color.Green).Padding(2, 1),
            new Panel(Align.Center(new Markup(
                $"[red]Rejected[/]\n\n[bold red]{importResult.RejectedCount}[/]\nMeasurements")))
                .Border(BoxBorder.Rounded).BorderColor(Color.Red).Padding(2, 1),
            new Panel(Align.Center(new Markup(
                $"[yellow]Duplicates[/]\n\n[bold yellow]{importResult.DuplicateCount}[/]\nMeasurements")))
                .Border(BoxBorder.Rounded).BorderColor(Color.Yellow).Padding(2, 1));

        AnsiConsole.Write(Align.Center(table));
        AnsiConsole.WriteLine();

        AnsiConsole.Write(Align.Center(new Markup(
            $"Total processed: {importResult.ImportedCount + importResult.RejectedCount + importResult.DuplicateCount} measurements")));
    }

    private void ShowLiveWeatherPanel(WeatherDataPoint weatherDataPoint)
    {
        var table = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand();

        table.AddColumn(new TableColumn("").Centered());
        table.AddColumn(new TableColumn("").Centered());

        table.AddRow("[deepskyblue1]🌡[/]", "[deepskyblue1]💧[/]");

        table.AddRow(
            "[deepskyblue1]Temperature[/]",
            "[deepskyblue1]Humidity[/]");

        table.AddRow(
            $"[bold white]{weatherDataPoint.Temperature:F1} °C[/]",
            $"[bold white]{weatherDataPoint.Humidity:F1} %[/]");

        var header = new Rows(
            Align.Center(new Markup(
                $"[bold deepskyblue1]{Markup.Escape(weatherDataPoint.StationName)}[/]")),
            Align.Center(new Markup(
                $"[grey]{weatherDataPoint.Timestamp:yyyy-MM-dd HH:mm} UTC[/]")),
            new Rule().RuleStyle("deepskyblue1"),
            new Markup(" "),
            table,
            new Markup(" "),
            new Rule().RuleStyle("deepskyblue1"),
            Align.Center(new Markup(
                "[grey]Data source: Open-Meteo API[/]")));

        AnsiConsole.Write(Align.Center(
            new Panel(header)
            {
                Width = Math.Min(50, Console.WindowWidth)
            }
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.DeepSkyBlue1)
                .Padding(2, 1)));
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
            AnsiConsole.Write(Align.Center(new Markup(
                "[red]Could not retrieve valid weather data.[/]")));
            AnsiConsole.WriteLine();

            return;
        }

        ShowLiveWeatherPanel(weatherDataPoint);

        AnsiConsole.WriteLine();

        if (_weatherDataService.IsDuplicate(weatherDataPoint))
        {
            AnsiConsole.Write(Align.Center(new Markup(
                "[yellow]Measurement already exists. No new data saved.[/]")));
            AnsiConsole.WriteLine();

            return;
        }

        _weatherDataService.SaveWeatherData([weatherDataPoint]);

        AnsiConsole.Write(Align.Center(new Markup(
            "[green]Weather measurement saved successfully.[/]")));
        AnsiConsole.WriteLine();
    }

    private string? SelectWeatherStation()
    {
        List<string> stations = _weatherDataService.GetUniqueStations();

        if (stations.Count == 0)
        {
            return null;
        }

        return SelectOption(
            stations.ToArray(), "WEATHER DATA STATISTICS", false);
    }

    private void ShowMeasurementsPanel(
        List<WeatherDataPoint> visibleMeasurements)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.DeepSkyBlue1)
            .Expand();

        table.AddColumn(new TableColumn("[deepskyblue1]Timestamp[/]").Centered());
        table.AddColumn(new TableColumn("[deepskyblue1]Temp. °C[/]").Centered());
        table.AddColumn(new TableColumn("[deepskyblue1]Humidity[/]").Centered());
        table.AddColumn(new TableColumn("[deepskyblue1]Source[/]").Centered());

        foreach (var measurement in visibleMeasurements)
        {
            table.AddRow(
                measurement.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                $"{measurement.Temperature:F1}",
                $"{measurement.Humidity:F1} %",
                Markup.Escape(measurement.DataSource));
        }

        AnsiConsole.Write(table);
    }

    private void ShowTemperatureSummaryPanel(
        List<WeatherDataPoint> stationMeasurements)
    {
        double? maxTemperature =
            _weatherDataService.CalculateMaxTemperature(stationMeasurements);

        double? minTemperature =
            _weatherDataService.CalculateMinTemperature(stationMeasurements);

        double? averageTemperature =
            _weatherDataService.CalculateAverageTemperature(stationMeasurements);

        var table = new Table()
            .NoBorder()
            .HideHeaders()
            .Expand();

        table.AddColumn(new TableColumn(""));
        table.AddColumn(new TableColumn(""));
        table.AddColumn(new TableColumn(""));

        table.AddRow(
            new Panel(Align.Center(new Markup(
                $"[red]Maximum[/]\n[bold white]{maxTemperature:F1} °C[/]")))
                .Border(BoxBorder.Rounded).BorderColor(Color.Red),
            new Panel(Align.Center(new Markup(
                $"[deepskyblue1]Minimum[/]\n[bold white]{minTemperature:F1} °C[/]")))
                .Border(BoxBorder.Rounded).BorderColor(Color.DeepSkyBlue1),
            new Panel(Align.Center(new Markup(
                $"[white]Average[/]\n[bold white]{averageTemperature:F1} °C[/]")))
                .Border(BoxBorder.Rounded).BorderColor(Color.White));

        AnsiConsole.Write(table);
    }

    private int GetPageSize()
    {
        int availableHeight = Console.WindowHeight - 16;
        return Math.Max(1, availableHeight);
    }

    private void DisplayPagination(
        int currentPage, int pageCount, int rowsPerPage,
        List<WeatherDataPoint> stationMeasurements)
    {
        AnsiConsole.Write(Align.Center(new Markup(
            $"[white]Page {currentPage + 1} of {pageCount}[/]")));
        AnsiConsole.Write(Align.Center(new Markup(
            $"[grey]Showing {currentPage * rowsPerPage + 1}–" +
            $"{Math.Min((currentPage + 1) * rowsPerPage, stationMeasurements.Count)} " +
            $"of {stationMeasurements.Count} measurements[/]")));
        AnsiConsole.Write(Align.Center(new Markup(
            "[grey]Left/Right: Change page · Esc: Main menu[/]")));
    }

    private bool DisplayWeatherStatistics()
    {
        string? stationName = SelectWeatherStation();

        if (stationName is null)
        {
            Console.Clear();
            ShowMenuHeader("WEATHER DATA STATISTICS", false);
            AnsiConsole.MarkupLine("[yellow]No weather stations found.[/]");
            return false;
        }

        List<WeatherDataPoint> stationMeasurements =
            _weatherDataService.GetStationMeasurements(stationName);

        if (stationMeasurements.Count == 0)
        {
            Console.Clear();
            ShowMenuHeader("WEATHER DATA STATISTICS", false);
            AnsiConsole.MarkupLine("[yellow]No measurements found for this station.[/]");
            return false;
        }

        int currentPage = 0;

        while (true)
        {
            int rowsPerPage = GetPageSize();
            int pageCount = (stationMeasurements.Count - 1) / rowsPerPage + 1;
            currentPage = Math.Min(currentPage, pageCount - 1);

            var visibleMeasurements = stationMeasurements
                .Skip(currentPage * rowsPerPage)
                .Take(rowsPerPage)
                .ToList();

            Console.Clear();
            ShowMenuHeader("WEATHER DATA STATISTICS", false);

            AnsiConsole.Write(Align.Center(new Markup(
                $"[bold deepskyblue1]{Markup.Escape(stationName)}[/]")));
            AnsiConsole.Write(Align.Center(new Markup(
                $"[grey]{stationMeasurements.Count} measurements[/]")));

            ShowTemperatureSummaryPanel(stationMeasurements);
            ShowMeasurementsPanel(visibleMeasurements);
            DisplayPagination(currentPage, pageCount, rowsPerPage, stationMeasurements);

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    currentPage = Math.Max(0, currentPage - 1);
                    break;

                case ConsoleKey.RightArrow:
                    currentPage = Math.Min(pageCount - 1, currentPage + 1);
                    break;

                case ConsoleKey.Escape:
                    return true;
            }
        }
    }
}
