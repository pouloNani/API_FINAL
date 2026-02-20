using System.Text.Json;

namespace Infrastructure.AI;

public static class JsonElementExtensions
{
    public static string? TryGet(this JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) ? v.GetString() : null;

    public static bool TryGetInt(this JsonElement el, string key, out int value)
    {
        if (el.TryGetProperty(key, out var v) && v.TryGetInt32(out value)) return true;
        value = 0;
        return false;
    }

    public static bool TryGetDouble(this JsonElement el, string key, out double value)
    {
        if (el.TryGetProperty(key, out var v) && v.TryGetDouble(out value)) return true;
        value = 0;
        return false;
    }

    public static bool TryGetDecimal(this JsonElement el, string key, out decimal value)
    {
        if (el.TryGetProperty(key, out var v) && v.TryGetDecimal(out value)) return true;
        value = 0;
        return false;
    }
}