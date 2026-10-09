using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Pages;

public class IndexModel : PageModel
{
    private readonly WeatherService _weatherService;

    private readonly ILogger<IndexModel> _logger;

    public IndexModel(WeatherService weatherService, ILogger<IndexModel> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    [BindProperty(SupportsGet = true)]
    public double? Lat { get; set; }

    [BindProperty(SupportsGet = true)]
    public double? Lon { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Place { get; set; }

    public string? ErrorMessage { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public CurrentWeather? Current { get; set; }
    public List<DailyForecastDay> DailyForecast { get; set; } = new();
    public List<GeocodingResult>? MultipleMatches { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(City))
        {
            // Default view on first load: show weather for New York City
            await LoadForecastAsync(40.7128, -74.0060, "New York, NY, United States");
            return;
        }

        // A specific match was already chosen from the disambiguation list
        if (Lat.HasValue && Lon.HasValue)
        {
            await LoadForecastAsync(Lat.Value, Lon.Value, Place ?? City);
            return;
        }

        List<GeocodingResult> matches;
        try
        {
            matches = await _weatherService.SearchCitiesAsync(City);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "City search failed for {City}", City);
            ErrorMessage = "Couldn't reach the weather service. Please try again in a moment.";
            return;
        }

        if (matches.Count == 0)
        {
            ErrorMessage = $"No cities found matching \"{City}\". Try a different spelling.";
            return;
        }

        if (matches.Count == 1)
        {
            var match = matches[0];
            await LoadForecastAsync(match.Latitude, match.Longitude, match.DisplayName);
            return;
        }

        // Multiple matches — let the user pick
        MultipleMatches = matches;
    }

    private async Task LoadForecastAsync(double lat, double lon, string placeName)
    {
        PlaceName = placeName;
        try
        {
            var forecast = await _weatherService.GetForecastAsync(lat, lon);
            Current = forecast?.Current;
            DailyForecast = _weatherService.BuildDailyView(forecast?.Daily);

            if (Current is null)
            {
                ErrorMessage = "Couldn't load current conditions for this location.";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Forecast failed for {Place} ({Lat}, {Lon})", placeName, lat, lon);
            ErrorMessage = "Couldn't reach the weather service. Please try again in a moment.";
        }
    }
}
