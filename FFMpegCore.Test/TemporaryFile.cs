using System;
using System.IO;

namespace FFMpegCore.Test
{
    public class TemporaryFile : IDisposable
    {
        private readonly string _path;

        public TemporaryFile(string filename)
        {
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}-{filename}");
        }

        public static implicit operator string(TemporaryFile temporaryFile) => temporaryFile._path;
        public void Dispose()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }
}