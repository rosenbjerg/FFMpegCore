using FFMpegCore.FFMPEG.Pipes;
using Instances;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents input parameter for a named pipe
    /// </summary>
    public class InputPipeArgument : Argument
    {
        public string PipeName { get; private set; }
        public string PipePath => $@"\\.\pipe\{PipeName}";
        public IPipeSource Source { get; private set; }

        private NamedPipeServerStream pipe;

        public InputPipeArgument(IPipeSource source)
        {
            Source = source;
            PipeName = "FFMpegCore_Pipe_" + Guid.NewGuid();
        }

        public void OpenPipe()
        {
            if (pipe != null)
                throw new InvalidOperationException("Pipe already has been opened");

            pipe = new NamedPipeServerStream(PipeName);
        }

        public void ClosePipe()
        {
            pipe?.Dispose();
            pipe = null;
        }

        public override string GetStringValue()
        {
            return $"-y {Source.GetFormat()} -i {PipePath}";
        }

        public void FlushPipe()
        {
            pipe.WaitForConnection();
            Source.FlushData(pipe);
        }


        public async Task FlushPipeAsync()
        {
            await pipe.WaitForConnectionAsync();
            await Source.FlushDataAsync(pipe);
        }
    }
}
