using System.Text.Json.Serialization;

namespace WeatherApp.Models;

public class GeocodingResponse
{
    [JsonPropertyName("results")]
    public List<GeocodingResult>? Results { get; set; }
}

public class GeocodingResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("admin1")]
    public string? Region { get; set; }

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Region) ? $"{Name}, {Country}" : $"{Name}, {Region}, {Country}";
}
