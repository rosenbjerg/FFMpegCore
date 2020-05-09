using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Instances;

namespace FFMpegCore.FFMPEG.Argument
{

    public interface INamedPipe
    {
        public void Open(PipeDirection direction);
        public Task During(CancellationToken cancellationToken);
        public void Close();
        System.IO.Stream GetStream();
        string PipePath { get; }
    }
    
    public class WindowsNamedPipe : INamedPipe
    {
        private readonly string _pipeName;

        public WindowsNamedPipe(string pipeName)
        {
            _pipeName = pipeName;
        }
        public void Open(PipeDirection direction)
        {
            if (Pipe != null)
                throw new InvalidOperationException("Pipe already has been opened");

            Pipe = new NamedPipeServerStream(_pipeName, direction, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public async Task During(CancellationToken cancellationToken)
        {
            await Pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            if (!Pipe.IsConnected)
                throw new TaskCanceledException();
        }

        public System.IO.Stream GetStream()
        {
            return Pipe;
        }

        public NamedPipeServerStream Pipe { get; set; }

        public void Close()
        {
            Pipe?.Dispose();
            Pipe = null;
        }
        public string PipePath => $@"\\.\pipe\{_pipeName}";
    }
    public class UnixNamedPipe : INamedPipe
    {
        private readonly string _pipeName;
        private PipeDirection _direction;

        public UnixNamedPipe(string pipeName)
        {
            _pipeName = pipeName;
        }
        
        public void Open(PipeDirection direction)
        {
            if (direction == PipeDirection.InOut)
                throw new NotImplementedException();
            _direction = direction;
            
            if (File.Exists(PipePath))
                throw new IOException($"Pipe name is already in use ({PipePath})");
            
            var (exitCode, _) = Instance.Finish("mkfifo", PipePath);
            if (exitCode != 0)
                throw new IOException($"Could not create FIFO file. (mkfifo failed with argument '{PipePath}')");
        }
        public Task During(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Close()
        {
            if (File.Exists(PipePath))
                File.Delete(PipePath);
        }

        public System.IO.Stream GetStream()
        {
            return _direction switch
            {
                PipeDirection.In => File.OpenRead(PipePath),
                PipeDirection.Out => File.OpenWrite(PipePath),
                _ => throw new NotImplementedException()
            };
        }

        public string PipePath => $"/tmp/CoreFxPipe_FIFO_{_pipeName}";
    }
}