using System.Globalization;

namespace FFMpegCore.Helpers;

internal static class NumberExtensions
{
    public static string ToInvariantString(this double value)
    {
        return value.ToString("0.##########", CultureInfo.InvariantCulture);
    }

    public static string ToInvariantString(this int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
