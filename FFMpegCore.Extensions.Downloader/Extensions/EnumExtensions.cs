using System.ComponentModel;

namespace FFMpegCore.Extensions.Downloader.Extensions;

public static class EnumExtensions
{
    internal static string GetDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field == null)
        {
            return enumValue.ToString();
        }

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
        {
            return attribute.Description;
        }

        return enumValue.ToString();
    }

    public static TEnum[] GetFlags<TEnum>(this TEnum input) where TEnum : Enum
    {
        return Enum.GetValues(input.GetType())
            .Cast<Enum>()
            .Where(input.HasFlag)
            .Cast<TEnum>()
            .ToArray();
    }
}
