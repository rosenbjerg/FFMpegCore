using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents threads parameter
    /// Number of threads used for video encoding
    /// </summary>
    public class ThreadsArgument : IArgument
    {
        public readonly int Threads;
        public ThreadsArgument(int threads)
        {
            Threads = threads;
        }

        public ThreadsArgument(bool isMultiThreaded) : this(isMultiThreaded ? Environment.ProcessorCount : 1) { }

        public string Text => $"-threads {Threads}";
    }
}
