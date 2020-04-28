using FFMpegCore.FFMPEG.Pipes;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    public class OutputPipeArgument : Argument
    {
        public string PipeName { get; private set; }
        public string PipePath => PipeHelpers.GetPipePath(PipeName);
        public IPipeDataReader Reader { get; private set; }

        private NamedPipeClientStream pipe;

        public OutputPipeArgument(IPipeDataReader reader)
        {
            Reader = reader;
            PipeName = PipeHelpers.GetUnqiuePipeName();
        }

        public void OpenPipe()
        {
            if(pipe != null)
                throw new InvalidOperationException("Pipe already has been opened");

            pipe = new NamedPipeClientStream(PipePath);
        }

        public void ReadData()
        {
            Reader.ReadData(pipe);
        }

        public Task ReadDataAsync()
        {
            return Reader.ReadDataAsync(pipe);
        }

        public void Close()
        {
            pipe?.Dispose();
            pipe = null;
        }

        public override string GetStringValue()
        {
            return $"\"{PipePath}\"";
        }
    }
}
