using System.Net.Http.Json;
using System.Text.Json;
using Core.Entities;

namespace Infrastructure.Services;

public class GeocodingService
{
    private readonly HttpClient _http;

    public GeocodingService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("API_FINAL/1.0");
    }

    public async Task<(double lat, double lng)?> GeocodeAsync(string address)
    {
        var url      = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
        var response = await _http.GetFromJsonAsync<JsonElement[]>(url);

        if (response == null || response.Length == 0) return null;

        var lat = double.Parse(response[0].GetProperty("lat").GetString()!);
        var lng = double.Parse(response[0].GetProperty("lon").GetString()!);
        return (lat, lng);
    }

    public async Task GeocodeShopAddressAsync(Shop shop)
    {
        if (shop.Address == null) return;

        var fullAddress = string.Join(", ",
            new string?[] { shop.Address.FirstLine, shop.Address.City, shop.Address.PostalCode.ToString(), shop.Address.Country }
            .Where(s => !string.IsNullOrEmpty(s)));

        var coords = await GeocodeAsync(fullAddress);
        if (coords.HasValue)
        {
            shop.Address.Latitude  = coords.Value.lat;
            shop.Address.Longitude = coords.Value.lng;
        }
    }
}