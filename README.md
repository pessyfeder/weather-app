# Skylight Weather

A small ASP.NET Core web app that shows current conditions and a 5-day
forecast for any city — built to demonstrate working with external APIs,
JSON deserialization, and async C# programming.

## Features
- Search any city worldwide by name
- Handles ambiguous matches (e.g. "Springfield" exists in many states) by
  letting you pick the right one
- Current conditions: temperature, feels-like, humidity, wind speed
- 5-day forecast strip with daily highs/lows
- No API key or signup required — uses the free [Open-Meteo](https://open-meteo.com) API

## Tech Stack
- ASP.NET Core 9.0 (Razor Pages)
- `HttpClient` + `System.Text.Json` for calling the Open-Meteo geocoding and forecast APIs
- Plain CSS (no external UI framework)

## Run it locally

**Prerequisites:** [.NET SDK](https://dotnet.microsoft.com/download) (matching whatever version is on your machine — run `dotnet --list-sdks` to check)

```bash
cd WeatherApp
dotnet restore
dotnet run
```

Then open the URL shown in the terminal (usually `https://localhost:5001` or `http://localhost:5000`).
The app loads showing New York City's weather by default — search any other city from there.

## Project structure

```
WeatherApp/
├── Services/
│   ├── WeatherService.cs      # Calls the Open-Meteo geocoding + forecast APIs
│   └── WeatherCodeMapper.cs   # Converts numeric weather codes to text/icons
├── Models/
│   ├── GeocodingModels.cs     # Shapes of the geocoding API's JSON response
│   └── WeatherModels.cs       # Shapes of the forecast API's JSON response
├── Pages/
│   ├── Index.cshtml           # Search box + results display
│   └── Shared/_Layout.cshtml
├── wwwroot/css/site.css
└── Program.cs
```

## How it works, at a glance

1. You type a city name and submit the search form.
2. `WeatherService.SearchCitiesAsync` calls Open-Meteo's **geocoding API** to turn
   that name into latitude/longitude (and a list of matches, if the name is ambiguous).
3. `WeatherService.GetForecastAsync` calls the **forecast API** with those coordinates.
4. The JSON response is automatically deserialized into C# objects (`ForecastResponse`, etc.)
   using `System.Text.Json`.
5. The Razor page displays the current conditions and loops through the 5-day forecast.

## Deploying it for free (so you can share a live link)

Same options as the Patient Tracker project:

### Option A — Render.com (easiest, free tier)
1. Push this project to a GitHub repo.
2. Create a free account at [render.com](https://render.com).
3. New → Web Service → connect your GitHub repo.
4. Build command: `dotnet publish -c Release -o out`
   Start command: `dotnet out/WeatherApp.dll`
5. Deploy. Render gives you a public `https://yourapp.onrender.com` URL.

### Option B — Azure App Service (free F1 tier)
```bash
az login
az webapp up --name your-weather-app-name --runtime "DOTNETCORE:9.0" --sku F1
```

No database or extra configuration needed for either option — this app has no
persistent storage, so it's about as simple a deploy as it gets.

## Resume bullet ideas (once deployed)

- Built a weather lookup web app in ASP.NET Core that consumes a third-party REST
  API (Open-Meteo) for geocoding and forecast data.
- Implemented async HTTP calls, JSON deserialization, and error handling for
  network failures and ambiguous search results.
- Designed a responsive, sky-themed UI in plain CSS with a 5-day forecast display.
