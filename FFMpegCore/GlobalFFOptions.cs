using System.Runtime.InteropServices;
using System.Text.Json;

namespace FFMpegCore;

public static class GlobalFFOptions
{
    private const string ConfigFile = "ffmpeg.config.json";
    private static FFOptions? _current;

    public static FFOptions Current => _current ??= LoadFFOptions();

    public static void Configure(Action<FFOptions> optionsAction)
    {
        optionsAction.Invoke(Current);
    }

    public static void Configure(FFOptions ffOptions)
    {
        _current = ffOptions ?? throw new ArgumentNullException(nameof(ffOptions));
    }

    public static string GetFFMpegBinaryPath(FFOptions? ffOptions = null)
    {
        return GetFFBinaryPath("FFMpeg", ffOptions ?? Current);
    }

    public static string GetFFProbeBinaryPath(FFOptions? ffOptions = null)
    {
        return GetFFBinaryPath("FFProbe", ffOptions ?? Current);
    }

    public static Task<string> GetFFMpegBinaryPathAsync(FFOptions? ffOptions = null, CancellationToken cancellationToken = default)
    {
        return GetFFBinaryPathAsync("FFMpeg", ffOptions ?? Current, cancellationToken);
    }

    public static Task<string> GetFFProbeBinaryPathAsync(FFOptions? ffOptions = null, CancellationToken cancellationToken = default)
    {
        return GetFFBinaryPathAsync("FFProbe", ffOptions ?? Current, cancellationToken);
    }

    private static string GetFFBinaryPath(string name, FFOptions ffOptions)
    {
        var ffName = GetFFName(name);

        foreach (var possiblePath in GetPossiblePaths(ffName, ffOptions))
        {
            if (File.Exists(possiblePath))
            {
                return possiblePath;
            }
        }

        //Fall back to the assumption this tool exists in the PATH
        return ffName;
    }

    private static async Task<string> GetFFBinaryPathAsync(string name, FFOptions ffOptions, CancellationToken cancellationToken = default)
    {
        var ffName = GetFFName(name);

        var results = await
            Task.WhenAll(
                GetPossiblePaths(ffName, ffOptions)
                    .Select(async possiblePath => await CheckPathAsync(possiblePath, cancellationToken).ConfigureAwait(false)))
            .ConfigureAwait(false);

        var foundPath = results.FirstOrDefault(path => path is not null);

        if (foundPath is not null)
        {
            return foundPath;
        }

        //Fall back to the assumption this tool exists in the PATH
        return ffName;
    }

    private static async Task<string?> CheckPathAsync(string possiblePath, CancellationToken cancellationToken)
    {
        var exists = await Task.Run(() => File.Exists(possiblePath), cancellationToken).ConfigureAwait(false);

        if (exists)
        {
            return possiblePath;
        }

        return null;
    }

    private static string GetFFName(string name)
    {
        var ffName = name.ToLowerInvariant();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ffName += ".exe";
        }

        return ffName;
    }

    private static HashSet<string> GetPossiblePaths(string ffName, FFOptions ffOptions)
    {
        var target = Environment.Is64BitProcess ? "x64" : "x86";

        var paths = new HashSet<string> { Path.Combine(ffOptions.BinaryFolder, target), ffOptions.BinaryFolder };

        return [.. paths.Select(possible => Path.Combine(possible, ffName))];
    }

    private static FFOptions LoadFFOptions()
    {
        return File.Exists(ConfigFile)
            ? JsonSerializer.Deserialize<FFOptions>(File.ReadAllText(ConfigFile))!
            : new FFOptions();
    }
}
