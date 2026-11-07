using FFMpegCore.Enums;

namespace FFMpegCore;

internal static class FFMpegCache
{
    private static readonly object _syncObject = new();
    private static readonly Dictionary<string, Dictionary<string, PixelFormat>> _pixelFormats = new();
    private static readonly Dictionary<string, Dictionary<string, Codec>> _codecs = new();
    private static readonly Dictionary<string, Dictionary<string, ContainerFormat>> _containers = new();

    public static IReadOnlyDictionary<string, PixelFormat> PixelFormats(FFOptions ffOptions)
    {
        return Get(_pixelFormats, ffOptions, options => FFMpeg.GetPixelFormatsInternal(options).ToDictionary(format => format.Name));
    }

    public static IReadOnlyDictionary<string, Codec> Codecs(FFOptions ffOptions)
    {
        return Get(_codecs, ffOptions, FFMpeg.GetCodecsInternal);
    }

    public static IReadOnlyDictionary<string, ContainerFormat> ContainerFormats(FFOptions ffOptions)
    {
        return Get(_containers, ffOptions, options => FFMpeg.GetContainersFormatsInternal(options).ToDictionary(format => format.Name));
    }

    private static IReadOnlyDictionary<string, TValue> Get<TValue>(Dictionary<string, Dictionary<string, TValue>> cache, FFOptions ffOptions,
        Func<FFOptions, Dictionary<string, TValue>> load)
    {
        var binaryPath = GlobalFFOptions.GetFFMpegBinaryPath(ffOptions);

        lock (_syncObject)
        {
            if (!cache.TryGetValue(binaryPath, out var entries))
            {
                entries = load(ffOptions);
                cache[binaryPath] = entries;
            }

            return entries;
        }
    }
}
