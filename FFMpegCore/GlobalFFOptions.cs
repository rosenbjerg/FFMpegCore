﻿using System.Runtime.InteropServices;
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

    private static string GetFFBinaryPath(string name, FFOptions ffOptions)
    {
        var ffName = name.ToLowerInvariant();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ffName += ".exe";
        }

        var target = Environment.Is64BitProcess ? "x64" : "x86";
        var possiblePaths = new List<string> { Path.Combine(ffOptions.BinaryFolder, target), ffOptions.BinaryFolder };

        foreach (var possiblePath in possiblePaths)
        {
            var possibleFFMpegPath = Path.Combine(possiblePath, ffName);
            if (File.Exists(possibleFFMpegPath))
            {
                return possibleFFMpegPath;
            }
        }

        //Fall back to the assumption this tool exists in the PATH
        return ffName;
    }

    private static FFOptions LoadFFOptions()
    {
        return File.Exists(ConfigFile)
            ? JsonSerializer.Deserialize<FFOptions>(File.ReadAllText(ConfigFile))!
            : new FFOptions();
    }
}
