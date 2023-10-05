using FFMpegCore.Arguments;
using FFMpegCore.Builders.MetaData;
using FFMpegCore.Pipes;

namespace FFMpegCore
{
    public sealed class FFMpegArguments : FFMpegArgumentsBase
    {
        private readonly FFMpegGlobalArguments _globalArguments = new();

        private FFMpegArguments() { }

        public string Text => GetText();

        private string GetText()
        {
            var allArguments = _globalArguments.Arguments.Concat(Arguments).ToArray();
            return string.Join(" ", allArguments.Select(arg => arg is IDynamicArgument dynArg ? dynArg.GetText(allArguments) : arg.Text));
        }

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
        public FFMpegArguments AddDeviceInput(string device, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputDeviceArgument(device), addArguments);
        public FFMpegArguments AddPipeInput(IPipeSource sourcePipe, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new InputPipeArgument(sourcePipe), addArguments);
        public FFMpegArguments AddMetaData(string content, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new MetaDataArgument(content), addArguments);
        public FFMpegArguments AddMetaData(IReadOnlyMetaData metaData, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new MetaDataArgument(MetaDataSerializer.Instance.Serialize(metaData)), addArguments);

        /// <summary>
        /// Maps the metadata of the given stream
        /// </summary>
        /// <param name="inputIndex">null means, the previous input will be used</param>
        public FFMpegArguments MapMetaData(int? inputIndex = null, Action<FFMpegArgumentOptions>? addArguments = null) => WithInput(new MapMetadataArgument(inputIndex), addArguments);

        private FFMpegArguments WithInput(IInputArgument inputArgument, Action<FFMpegArgumentOptions>? addArguments)
        {
            var arguments = new FFMpegArgumentOptions();
            addArguments?.Invoke(arguments);
            Arguments.AddRange(arguments.Arguments);
            Arguments.Add(inputArgument);
            return this;
        }

        public FFMpegArgumentProcessor OutputToFile(string file, bool overwrite = true, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputArgument(file, overwrite), addArguments);
        public FFMpegArgumentProcessor OutputToUrl(string uri, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputUrlArgument(uri), addArguments);
        public FFMpegArgumentProcessor OutputToUrl(Uri uri, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputUrlArgument(uri.ToString()), addArguments);
        public FFMpegArgumentProcessor OutputToPipe(IPipeSink reader, Action<FFMpegArgumentOptions>? addArguments = null) => ToProcessor(new OutputPipeArgument(reader), addArguments);

        private FFMpegArgumentProcessor ToProcessor(IOutputArgument argument, Action<FFMpegArgumentOptions>? addArguments)
        {
            var args = new FFMpegArgumentOptions();
            addArguments?.Invoke(args);
            Arguments.AddRange(args.Arguments);
            Arguments.Add(argument);
            return new FFMpegArgumentProcessor(this);
        }

        public FFMpegArgumentProcessor OutputToTee(Action<FFMpegMultiOutputOptions> addOutputs, Action<FFMpegArgumentOptions>? addArguments = null)
        {
            var outputs = new FFMpegMultiOutputOptions();
            addOutputs(outputs);
            return ToProcessor(new OutputTeeArgument(outputs), addArguments);
        }

        public FFMpegArgumentProcessor MultiOutput(Action<FFMpegMultiOutputOptions> addOutputs)
        {
            var args = new FFMpegMultiOutputOptions();
            addOutputs(args);
            Arguments.AddRange(args.Arguments);
            return new FFMpegArgumentProcessor(this);
        }

        internal void Pre()
        {
            foreach (var argument in Arguments.OfType<IInputOutputArgument>())
            {
                argument.Pre();
            }
        }
        internal async Task During(CancellationToken cancellationToken = default)
        {
            var inputOutputArguments = Arguments.OfType<IInputOutputArgument>();
            await Task.WhenAll(inputOutputArguments.Select(io => io.During(cancellationToken))).ConfigureAwait(false);
        }
        internal void Post()
        {
            foreach (var argument in Arguments.OfType<IInputOutputArgument>())
            {
                argument.Post();
            }
        }
    }
}
