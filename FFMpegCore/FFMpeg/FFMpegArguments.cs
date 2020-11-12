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
    public sealed class FFMpegArguments : FFMpegOptionsBase
    {
        private readonly FFMpegGlobalOptions _globalOptions = new FFMpegGlobalOptions();
        
        private FFMpegArguments() { }

        public string Text => string.Join(" ", _globalOptions.Arguments.Concat(Arguments).Select(arg => arg.Text));

        public static FFMpegArguments FromConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new ConcatArgument(filePaths), addArguments);
        public static FFMpegArguments FromDemuxConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new DemuxConcatArgument(filePaths), addArguments);
        public static FFMpegArguments FromFileInput(string filePath, bool verifyExists = true, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(verifyExists, filePath), addArguments);
        public static FFMpegArguments FromFileInput(FileInfo fileInfo, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(fileInfo.FullName, false), addArguments);
        public static FFMpegArguments FromFileInput(IMediaAnalysis mediaAnalysis, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(mediaAnalysis.Path, false), addArguments);
        public static FFMpegArguments FromUrlInput(Uri uri, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
        public static FFMpegArguments FromPipeInput(IPipeSource sourcePipe, Action<FFMpegArgumentOptions>? addArguments = null) => new FFMpegArguments().WithInput(new InputPipeArgument(sourcePipe), addArguments);

        
        public FFMpegArguments WithGlobalOptions(Action<FFMpegGlobalOptions> configureOptions)
        {
            configureOptions(_globalOptions);
            return this;
        }

        public FFMpegArguments AddConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new ConcatArgument(filePaths), addArguments);
        public FFMpegArguments AddDemuxConcatInput(IEnumerable<string> filePaths, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new DemuxConcatArgument(filePaths), addArguments);
        public FFMpegArguments AddFileInput(string filePath, bool verifyExists = true, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputArgument(verifyExists, filePath), addArguments);
        public FFMpegArguments AddFileInput(FileInfo fileInfo, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputArgument(fileInfo.FullName, false), addArguments);
        public FFMpegArguments AddFileInput(IMediaAnalysis mediaAnalysis, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputArgument(mediaAnalysis.Path, false), addArguments);
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