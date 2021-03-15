using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Arguments;
using FFMpegCore.Pipes;

namespace FFMpegCore
{
    public sealed class FFMpegArguments : FFMpegArgumentsBase
    {
        private readonly FFMpegGlobalArguments _globalArguments = new FFMpegGlobalArguments();
        
        private FFMpegArguments() { }

        public string Text => string.Join(" ", _globalArguments.Arguments.Concat(Arguments).Select(arg => arg.Text));

        public static FFMpegArguments FromConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new ConcatArgument(filePaths), addArguments);
        public static FFMpegArguments FromDemuxConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new DemuxConcatArgument(filePaths), addArguments);
        public static FFMpegArguments FromFileInput(string filePath, bool verifyExists = true, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(verifyExists, filePath), addArguments);
        public static FFMpegArguments FromFileInput(FileInfo fileInfo, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(fileInfo.FullName, false), addArguments);
        public static FFMpegArguments FromUrlInput(Uri uri, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
        public static FFMpegArguments FromDeviceInput(string device, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputDeviceArgument(device), addArguments);
        public static FFMpegArguments FromPipeInput(IPipeSource sourcePipe, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputPipeArgument(sourcePipe), addArguments);

        
        public FFMpegArguments WithGlobalOptions(Action<FFMpegGlobalArguments> configureOptions)
        {
            configureOptions(_globalArguments);
            return this;
        }

        public FFMpegArguments AddConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new ConcatArgument(filePaths), addArguments);
        public FFMpegArguments AddDemuxConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new DemuxConcatArgument(filePaths), addArguments);
        public FFMpegArguments AddFileInput(string filePath, bool verifyExists = true, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputArgument(verifyExists, filePath), addArguments);
        public FFMpegArguments AddFileInput(FileInfo fileInfo, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputArgument(fileInfo.FullName, false), addArguments);
        public FFMpegArguments AddUrlInput(Uri uri, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
        public FFMpegArguments AddPipeInput(IPipeSource sourcePipe, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputPipeArgument(sourcePipe), addArguments);

        private FFMpegArguments WithInput(IInputArgument inputArgument, Action<FFMpegArgumentOptions>? addArguments)
        {
            var arguments = new FFMpegArgumentOptions();
            addArguments?.Invoke(arguments);
            Arguments.AddRange(arguments.Arguments);
            Arguments.Add(inputArgument);
            return this;
        }

        public FFMpegArgumentProcessor OutputToFile(string file, bool overwrite = true, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputArgument(file, overwrite), addArguments);
        public FFMpegArgumentProcessor OutputToFile(Uri uri, bool overwrite = true, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputArgument(uri.AbsolutePath, overwrite), addArguments);
        public FFMpegArgumentProcessor OutputToStream(string uri, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputStreamArgument(uri), addArguments);
        public FFMpegArgumentProcessor OutputToStream(Uri uri, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputStreamArgument(uri.ToString()), addArguments);
        public FFMpegArgumentProcessor OutputToPipe(IPipeSink reader, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputPipeArgument(reader), addArguments);

        private FFMpegArgumentProcessor ToProcessor(IOutputArgument argument, Action<FFMpegArgumentOptions>? addArguments)
        {
            var args = new FFMpegArgumentOptions();
            addArguments?.Invoke(args);
            Arguments.AddRange(args.Arguments);
            Arguments.Add(argument);
            return new FFMpegArgumentProcessor(this);
        }

        internal void Pre()
        {
            foreach (var argument in Arguments.OfType<IInputOutputArgument>())
                argument.Pre();
        }
        internal async Task During(CancellationToken cancellationToken = default)
        {
            var inputOutputArguments = Arguments.OfType<IInputOutputArgument>();
            await Task.WhenAll(inputOutputArguments.Select(io => io.During(cancellationToken))).ConfigureAwait(false);
        }
        internal void Post()
        {
            foreach (var argument in Arguments.OfType<IInputOutputArgument>())
                argument.Post();
        }
    }
}