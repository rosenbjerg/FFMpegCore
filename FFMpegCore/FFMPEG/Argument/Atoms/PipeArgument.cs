using FFMpegCore.FFMPEG.Pipes;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Unix;

namespace FFMpegCore.FFMPEG.Argument
{
    public abstract class PipeArgument : Argument
    {
        public string PipePath => Pipe.PipePath;

        protected INamedPipe Pipe { get; private set; }
        private PipeDirection direction;

        protected PipeArgument(PipeDirection direction)
        {
            var pipeName = "FFMpegCore_Pipe_" + Guid.NewGuid();
            Pipe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) switch
            {
                true => new WindowsNamedPipe(pipeName),
                false => new UnixNamedPipe(pipeName)
            };
            this.direction = direction;
        }

        public void OpenPipe()
        {
            Pipe.Open(direction);
        }

        public void ClosePipe()
        {
            Pipe.Close();
        }
        public Task ProcessDataAsync()
        {
            return ProcessDataAsync(CancellationToken.None);
        }

        public abstract Task ProcessDataAsync(CancellationToken token);
    }
}
