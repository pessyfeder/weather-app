using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class WeatherService
{
    private static readonly TimeSpan ForecastCacheTime = TimeSpan.FromMinutes(10);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient http, IMemoryCache cache, ILogger<WeatherService> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    // Open-Meteo's geocoding API turns a city name into latitude/longitude.
    // No API key required.
    public async Task<List<GeocodingResult>> SearchCitiesAsync(string cityName)
    {
        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}&count=5&language=en&format=json";
        var response = await GetJsonWithRetryAsync<GeocodingResponse>(url);
        return response?.Results ?? new List<GeocodingResult>();
    }

    // Fetches current conditions + a 5-day forecast for a given coordinate.
    // Results are cached for a few minutes: the host's shared outbound IP can hit
    // Open-Meteo's free-tier rate limit, and conditions don't change that fast.
    public async Task<ForecastResponse?> GetForecastAsync(double latitude, double longitude)
    {
        var key = $"forecast:{Math.Round(latitude, 2)}:{Math.Round(longitude, 2)}";
        if (_cache.TryGetValue(key, out ForecastResponse? cached)) return cached;

        var forecast = await FetchForecastAsync(latitude, longitude);
        if (forecast is not null) _cache.Set(key, forecast, ForecastCacheTime);
        return forecast;
    }

    private async Task<ForecastResponse?> FetchForecastAsync(double latitude, double longitude)
    {
        var url = "https://api.open-meteo.com/v1/forecast" +
                   $"?latitude={latitude}&longitude={longitude}" +
                   "&current=temperature_2m,apparent_temperature,relative_humidity_2m,wind_speed_10m,precipitation,weather_code" +
                   "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                   "&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch" +
                   "&timezone=auto&forecast_days=5";

        return await GetJsonWithRetryAsync<ForecastResponse>(url);
    }

    // One retry after a short pause on rate limiting (429), a server error (5xx) or a
    // connection failure. Failures are logged with the status code and response body
    // so they can be diagnosed from the host's logs.
    private async Task<T?> GetJsonWithRetryAsync<T>(string url)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                var body = await response.Content.ReadAsStringAsync();
                var transient = response.StatusCode == HttpStatusCode.TooManyRequests
                                || (int)response.StatusCode >= 500;
                _logger.LogWarning("Open-Meteo {Url} returned {Status} (attempt {Attempt}): {Body}",
                    url, (int)response.StatusCode, attempt, body);

                if (transient && attempt == 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1.5));
                    continue;
                }
                throw new HttpRequestException(
                    $"Open-Meteo returned {(int)response.StatusCode}: {body}", null, response.StatusCode);
            }
            catch (HttpRequestException ex) when (ex.StatusCode is null)
            {
                _logger.LogWarning(ex, "Open-Meteo request to {Url} failed (attempt {Attempt})", url, attempt);
                if (attempt == 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1.5));
                    continue;
                }
                throw;
            }
        }
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
