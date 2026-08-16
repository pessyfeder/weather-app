using System.Net.Http.Json;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class WeatherService
{
    private readonly HttpClient _http;

    public WeatherService(HttpClient http)
    {
        _http = http;
    }

    // Open-Meteo's geocoding API turns a city name into latitude/longitude.
    // No API key required.
    public async Task<List<GeocodingResult>> SearchCitiesAsync(string cityName)
    {
        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}&count=5&language=en&format=json";
        var response = await _http.GetFromJsonAsync<GeocodingResponse>(url);
        return response?.Results ?? new List<GeocodingResult>();
    }

    // Fetches current conditions + a 5-day forecast for a given coordinate.
    public async Task<ForecastResponse?> GetForecastAsync(double latitude, double longitude)
    {
        var url = "https://api.open-meteo.com/v1/forecast" +
                   $"?latitude={latitude}&longitude={longitude}" +
                   "&current=temperature_2m,apparent_temperature,relative_humidity_2m,wind_speed_10m,precipitation,weather_code" +
                   "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                   "&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch" +
                   "&timezone=auto&forecast_days=5";

        return await _http.GetFromJsonAsync<ForecastResponse>(url);
    }

    public List<DailyForecastDay> BuildDailyView(DailyForecast? daily)
    {
        var days = new List<DailyForecastDay>();
        if (daily is null) return days;

        for (int i = 0; i < daily.Dates.Count; i++)
        {
            days.Add(new DailyForecastDay
            {
                Date = DateTime.Parse(daily.Dates[i]),
                TempMax = daily.TempMax[i],
                TempMin = daily.TempMin[i],
                PrecipitationChance = i < daily.PrecipitationChance.Count ? daily.PrecipitationChance[i] : 0,
                Description = WeatherCodeMapper.Describe(daily.WeatherCodes[i]),
                Icon = WeatherCodeMapper.Icon(daily.WeatherCodes[i]),
            });
        }
        return days;
    }
}
