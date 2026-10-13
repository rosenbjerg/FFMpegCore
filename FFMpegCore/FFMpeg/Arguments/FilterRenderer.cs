namespace FFMpegCore.Arguments;

internal static class FilterRenderer
{
    public static bool HasText(string key, string value)
    {
        return !string.IsNullOrEmpty(key) || !string.IsNullOrEmpty(value);
    }

    public static string Render(string key, string value, bool escapeCommas)
    {
        if (escapeCommas)
        {
            value = value.Replace(",", "\\,");
        }

        if (string.IsNullOrEmpty(key))
        {
            return value;
        }

        return string.IsNullOrEmpty(value) ? key : $"{key}={value}";
    }
}
