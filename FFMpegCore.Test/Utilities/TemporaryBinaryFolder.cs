using System.Runtime.Versioning;

namespace FFMpegCore.Test.Utilities;

[UnsupportedOSPlatform("windows")]
internal sealed class TemporaryBinaryFolder : IDisposable
{
    private readonly string _folder;

    public TemporaryBinaryFolder(string binaryName)
    {
        _folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_folder);

        BinaryPath = Path.Combine(_folder, binaryName);
        File.WriteAllText(BinaryPath, "#!/bin/sh\nexit 1\n");
        File.SetUnixFileMode(BinaryPath, UnixFileMode.UserRead | UnixFileMode.UserExecute);

        Options = new FFOptions { BinaryFolder = _folder };
    }

    public string BinaryPath { get; }

    public FFOptions Options { get; }

    public void Dispose()
    {
        Directory.Delete(_folder, true);
    }
}
