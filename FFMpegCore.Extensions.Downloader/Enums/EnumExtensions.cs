namespace FFMpegCore.Extensions.Downloader.Enums;

public static class EnumExtensions
{
    public static TEnum[] GetFlags<TEnum>(this TEnum input) where TEnum : Enum
    {
        return Enum.GetValues(input.GetType())
            .Cast<Enum>()
            .Where(input.HasFlag)
            .Cast<TEnum>()
            .ToArray();
    }
}
