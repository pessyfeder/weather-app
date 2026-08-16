using System.Text.Json.Serialization;

namespace WeatherApp.Models;

public class ForecastResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }

    [JsonPropertyName("daily")]
    public DailyForecast? Daily { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public int Humidity { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}

public class DailyForecast
{
    [JsonPropertyName("time")]
    public List<string> Dates { get; set; } = new();

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCodes { get; set; } = new();

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TempMax { get; set; } = new();

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TempMin { get; set; } = new();

    [JsonPropertyName("precipitation_probability_max")]
    public List<int> PrecipitationChance { get; set; } = new();
}

// Flattened view model used by the Razor page
public class DailyForecastDay
{
    public DateTime Date { get; set; }
    public double TempMax { get; set; }
    public double TempMin { get; set; }
    public int PrecipitationChance { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
