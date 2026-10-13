using System.Globalization;

namespace FFMpegCore.Helpers;

internal static class DoubleExtensions
{
    public static string ToInvariantString(this double value)
    {
        return value.ToString("0.##########", CultureInfo.InvariantCulture);
    }
}
