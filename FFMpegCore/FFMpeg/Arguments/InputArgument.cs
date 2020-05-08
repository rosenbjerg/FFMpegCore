using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents input parameter
    /// </summary>
    public class InputArgument : IInputArgument
    {
        public readonly bool VerifyExists;
        public readonly string[] FilePaths;
        
        public InputArgument(bool verifyExists, params string[] filePaths)
        {
            VerifyExists = verifyExists;
            FilePaths = filePaths;
        }

        public InputArgument(params string[] filePaths) : this(true, filePaths) { }
        public InputArgument(params FileInfo[] fileInfos) : this(false, fileInfos) { }
        public InputArgument(params Uri[] uris) : this(false, uris) { }
        public InputArgument(bool verifyExists, params FileInfo[] fileInfos) : this(verifyExists, fileInfos.Select(v => v.FullName).ToArray()) { }
        public InputArgument(bool verifyExists, params Uri[] uris) : this(verifyExists, uris.Select(v => v.AbsoluteUri).ToArray()) { }

        public void Pre()
        {
            if (!VerifyExists) return;
            foreach (var filePath in FilePaths)
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Input file not found", filePath);
            }
        }

        public string Text => string.Join(" ", FilePaths.Select(v => $"-i \"{v}\""));
    }

    public interface IArgument
    {
        string Text { get; }
    }

    public interface IInputArgument : IArgument
    {
        void Pre() {}
        Task During(CancellationToken? cancellationToken = null) => Task.CompletedTask;
        void Post() {}
    }
    public interface IOutputArgument : IArgument
    {
        void Pre() {}
        Task During(CancellationToken? cancellationToken = null) => Task.CompletedTask;
        void Post() {}
    }
}
