using System.ComponentModel;
using System.Diagnostics;
using Instances.Exceptions;

namespace FFMpegCore.Helpers;

internal static class ProcessHelper
{
    private const int FileNotFoundErrorCode = 2;

    // Reads on the calling thread on purpose: Instances drains stdout through the thread pool, and
    // callers block pool threads on this (under FFMpegCache's lock) until it returns — see #580.
    public static (int ExitCode, IReadOnlyList<string> OutputData) Run(string fileName, string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        try
        {
            process.Start();
        }
        catch (Win32Exception e) when (e.NativeErrorCode == FileNotFoundErrorCode)
        {
            throw new InstanceFileNotFoundException(fileName, e);
        }

        var errorDrain = new Thread(() => process.StandardError.ReadToEnd()) { IsBackground = true };
        errorDrain.Start();

        var outputData = new List<string>();
        while (process.StandardOutput.ReadLine() is { } line)
        {
            outputData.Add(line);
        }

        errorDrain.Join();
        process.WaitForExit();
        return (process.ExitCode, outputData);
    }
}
