

\# FailSafe WeatherMonitor





\## Projektbeskrivning



FailSafe WeatherMonitor är en konsolapplikation utvecklad i C# för att samla in, validera, lagra och presentera historisk och aktuell väderdata.



Applikationen är utformad som ett administrativt verktyg för drifttekniker som behöver övervaka temperatur och luftfuktighet.



Mätdata hämtas från en lokal CSV-fil och Open-Meteo API och lagras i SQL Server med Entity Framework Core.





\---





\## Funktionalitet



\### Import av historisk väderdata



\- Läser in den medföljande datafilen med 200 mätningar.

\- Validerar stationsnamn, tidstämpel, temperatur och luftfuktighet.

\- Förkastar felaktiga mätningar.

\- Loggar korrupta datarader i `bad\_data.log`.

\- Kontrollerar och förhindrar duplicerade mätningar.

\- Sparar giltiga mätningar i databasen.

\- Visar antal importerade, förkastade och duplicerade mätningar.





\### Hämtning av aktuell väderdata



\- Hämtar väderdata från Open-Meteo via HttpClient.

\- Bearbetar API-svaret i JSON-format.

\- Hämtar aktuell temperatur och luftfuktighet för Göteborg.

\- Validerar mätvärden och kontrollerar dubbletter före lagring.

\- Sparar giltiga mätningar i databasen.

\- Presenterar aktuella mätvärden i konsolen.

\- Hanterar nätverksfel.





\### Väderstatistik



\- Listar samtliga unika mätstationer från databasen och låter användaren välja en.

\- Visar vald stations mätningar i kronologisk ordning.

\- Beräknar högsta, lägsta och genomsnittliga temperatur.

\- Presenterar mätvärdena i överskådliga tabeller med dynamisk sidindelning.





\---





\## Tekniker



\- C# och .NET 11

\- Entity Framework Core 11 (Code First och Migrations)

\- SQL Server LocalDB

\- LINQ

\- HttpClient och System.Text.Json

\- Spectre.Console

\- Git och GitHub





\---





\## Installation och uppstart



\### Förutsättningar



\- Visual Studio med stöd för .NET 11.

\- .NET 11 SDK.

\- SQL Server LocalDB.





\### Installation



1\. Klona eller ladda ner projektet från GitHub.

2\. Öppna `FailSafeWeatherMonitor.slnx` i Visual Studio.

3\. Återställ projektets NuGet-paket.

4\. Kontrollera anslutningssträngen i `appsettings.json`.

5\. Öppna Package Manager Console.

6\. Kontrollera att FailSafeWeatherMonitor.ConsoleApp är valt som Default project och startprojekt.

7\. Kör följande kommando för att skapa databasen:



&#x20;  ```powershell

&#x20;  Update-Database

&#x20;  ```



8\. Starta applikationen med Ctrl + F5.





\---





\## Databas



\- Databasinstans: `(localdb)\\MSSQLLocalDB`

\- Databasnamn: `FailSafeWeatherMonitorDb`

\- Tabell: `WeatherDataPoints`



Databasschemat versionshanteras med EF Core Migrations. Anslutningssträngen konfigureras i `appsettings.json` och använder Windows-autentisering.





\---





\## Kodstruktur



Applikationen är uppdelad i separata klasser för databashantering, filimport, API-kommunikation, datavalidering och användargränssnitt.



Gemensam valideringslogik återanvänds för båda datakällorna.





\---





\## Datavalidering och felhantering



Data från CSV-filen valideras med `TryParse`. Temperatur och luftfuktighet rimlighetskontrolleras för båda datakällorna före lagring.



Felaktiga datarader förkastas och sparas i `bad\_data.log`, som skrivs över vid varje genomförd import. Loggfilen sparas i programmets aktuella arbetskatalog.



Filfel och nätverksfel hanteras med `try-catch`, vilket gör att användaren kan återgå till huvudmenyn utan att applikationen kraschar.





\---





\## Testning



Applikationen kompilerades och funktionstestades 



\### Import och datavalidering



| Kontroll | Resultat |

|---|---:|

| Bearbetade mätningar | 200 |

| Giltiga mätningar | 186 |

| Förkastade mätningar | 14 |

| Loggade felrader i bad\_data.log | 14 |

| Dubbletter vid upprepad import | 186 |

| Nya mätningar vid upprepad import | 0 |





\### Funktionstester



| Testområde | Resultat |

|---|---|

| Kompilering | Inga fel eller varningar i byggutskriften |

| Databas | Lagring och hämtning av mätvärden fungerar |

| API-integration | Väderdata hämtas och valideras |

| Dubbletthantering | Befintliga mätningar sparas inte på nytt |

| Nätverksfel | Felmeddelande visas utan programkrasch |

| Saknad datafil | Felmeddelande visas utan programkrasch |

| Väderstatistik | Filtrering, sortering och temperaturberäkningar fungerar |

| Sidindelning | Navigering mellan sidor fungerar |

| Avslutning | Applikationen avslutas med kod 0 |



Samtliga genomförda funktionstester gav förväntat resultat.





\---





\## Skärmbilder



\### Huvudmeny



!\[Huvudmeny](ProgramPictures/startmenu.png)





\### Import av historisk väderdata



!\[CSV-import](ProgramPictures/csv-import.png)





\### Aktuell väderdata från Open-Meteo



!\[Aktuell väderdata](ProgramPictures/live-weather.png)





\### Väderstatistik



!\[Väderstatistik](ProgramPictures/weather-statistics.png)





\### Sidindelning



!\[Sida 1](ProgramPictures/statistics-page-1.png)



!\[Sida 2](ProgramPictures/statistics-page-2.png)





\### Felloggning



!\[Felloggning](ProgramPictures/bad-data-log.png)





\### Hantering av nätverksfel



!\[Nätverksfel](ProgramPictures/network-error.png)





\---





\## Extern datakälla



Väderdata hämtas från Open-Meteo Weather API.



\- \[Open-Meteo](https://open-meteo.com/)

\- \[API-dokumentation](https://open-meteo.com/en/docs)





\---





\## Projektinformation



\- Kurs: Databashantering SQL

\- Uppgift: Inlämningsuppgift 2 - FailSafe WeatherMonitor

\- Utvecklare: Håkan Täpp Andersson



