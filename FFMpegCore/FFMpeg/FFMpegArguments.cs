using FFMpegCore.Arguments;
using FFMpegCore.Pipes;

namespace FFMpegCore;

public sealed class FFMpegArguments : FFMpegArgumentsBase
{
    private FFMpegArguments() { }

    internal string Text => GetText();

    private string GetText()
    {
        var allArguments = Arguments.ToArray();
        return string.Join(" ", allArguments
            .Select(arg => arg is IDynamicArgument dynArg ? dynArg.GetText(allArguments) : arg.Text)
            .Where(text => !string.IsNullOrEmpty(text)));
    }

    public static FFMpegArguments FromInput(IInputArgument input, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(input, addArguments);
    }

    public static FFMpegArguments FromConcatProtocolInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new ConcatProtocolArgument(filePaths), addArguments);
    }

    public static FFMpegArguments FromConcatDemuxerInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new ConcatDemuxerArgument(filePaths), addArguments);
    }

    public static FFMpegArguments FromFileInput(string filePath, Action<FFMpegInputOptions> addArguments)
    {
        return FromFileInput(filePath, true, addArguments);
    }

    public static FFMpegArguments FromFileInput(string filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(filePath, verifyExists), addArguments);
    }

    public static FFMpegArguments FromFileInputs(IEnumerable<string> filePaths, Action<FFMpegInputOptions> addArguments)
    {
        return FromFileInputs(filePaths, true, addArguments);
    }

    public static FFMpegArguments FromFileInputs(IEnumerable<string> filePaths, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().AddFileInputs(filePaths, verifyExists, addArguments);
    }

    public static FFMpegArguments FromFileInput(FileInfo fileInfo, Action<FFMpegInputOptions> addArguments)
    {
        return FromFileInput(fileInfo, true, addArguments);
    }

    public static FFMpegArguments FromFileInput(FileInfo fileInfo, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(fileInfo.FullName, verifyExists), addArguments);
    }

    public static FFMpegArguments FromUrlInput(Uri uri, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
    }

    public static FFMpegArguments FromUrlInput(string uri, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(uri, false), addArguments);
    }

    public static FFMpegArguments FromDeviceInput(string device, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputDeviceArgument(device), addArguments);
    }

    public static FFMpegArguments FromPipeInput(IPipeSource sourcePipe, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputPipeArgument(sourcePipe), addArguments);
    }

    public static FFMpegArguments FromImageSequenceInput(IEnumerable<string> images, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new ImageSequenceInputArgument(images), addArguments);
    }

    public FFMpegArguments AddInput(IInputArgument input, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(input, addArguments);
    }

    public FFMpegArguments AddConcatProtocolInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new ConcatProtocolArgument(filePaths), addArguments);
    }

    public FFMpegArguments AddConcatDemuxerInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new ConcatDemuxerArgument(filePaths), addArguments);
    }

    public FFMpegArguments AddFileInput(string filePath, Action<FFMpegInputOptions> addArguments)
    {
        return AddFileInput(filePath, true, addArguments);
    }

    public FFMpegArguments AddFileInput(string filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(filePath, verifyExists), addArguments);
    }

    public FFMpegArguments AddFileInputs(IEnumerable<string> filePaths, Action<FFMpegInputOptions> addArguments)
    {
        return AddFileInputs(filePaths, true, addArguments);
    }

    public FFMpegArguments AddFileInputs(IEnumerable<string> filePaths, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return filePaths.Aggregate(this, (arguments, path) => arguments.WithInput(new InputArgument(path, verifyExists), addArguments));
    }

    public FFMpegArguments AddFileInput(FileInfo fileInfo, Action<FFMpegInputOptions> addArguments)
    {
        return AddFileInput(fileInfo, true, addArguments);
    }

    public FFMpegArguments AddFileInput(FileInfo fileInfo, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(fileInfo.FullName, verifyExists), addArguments);
    }

    public FFMpegArguments AddUrlInput(Uri uri, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
    }

    public FFMpegArguments AddUrlInput(string uri, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(uri, false), addArguments);
    }

    public FFMpegArguments AddDeviceInput(string device, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputDeviceArgument(device), addArguments);
    }

    public FFMpegArguments AddPipeInput(IPipeSource sourcePipe, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputPipeArgument(sourcePipe), addArguments);
    }

    public FFMpegArguments AddImageSequenceInput(IEnumerable<string> images, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new ImageSequenceInputArgument(images), addArguments);
    }

    public FFMpegArguments AddMetadata(FFMetadataBuilder metadataBuilder, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new MetadataArgument(metadataBuilder.Build()), addArguments);
    }

    public FFMpegArguments AddMetadataFile(string filePath, Action<FFMpegInputOptions> addArguments)
    {
        return AddMetadataFile(filePath, true, addArguments);
    }

    public FFMpegArguments AddMetadataFile(string filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new MetadataFileArgument(filePath, verifyExists), addArguments);
    }

    private FFMpegArguments WithInput(IInputArgument inputArgument, Action<FFMpegInputOptions>? addArguments)
    {
        var arguments = new FFMpegInputOptions();
        addArguments?.Invoke(arguments);
        Arguments.AddRange(arguments.Arguments);
        Arguments.Add(inputArgument);
        return this;
    }

    public FFMpegArgumentProcessor OutputToFile(string file, Action<FFMpegOutputOptions> addArguments)
    {
        return OutputToFile(file, true, addArguments);
    }

    public FFMpegArgumentProcessor OutputToFile(string file, bool overwrite = true, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return ToProcessor(new OutputArgument(file, overwrite), addArguments);
    }

    public FFMpegArgumentProcessor OutputToUrl(string uri, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return ToProcessor(new OutputUrlArgument(uri), addArguments);
    }

    public FFMpegArgumentProcessor OutputToUrl(Uri uri, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return ToProcessor(new OutputUrlArgument(uri.AbsoluteUri), addArguments);
    }

    public FFMpegArgumentProcessor OutputToPipe(IPipeSink reader, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return ToProcessor(new OutputPipeArgument(reader), addArguments);
    }

    public FFMpegArgumentProcessor OutputToNull(Action<FFMpegOutputOptions>? addArguments = null)
    {
        return ToProcessor(new NullOutputArgument(), addArguments);
    }

    private FFMpegArgumentProcessor ToProcessor(IOutputArgument argument, Action<FFMpegOutputOptions>? addArguments)
    {
        var args = new FFMpegOutputOptions();
        addArguments?.Invoke(args);
        MapAddedMetadata(args.Arguments);
        Arguments.AddRange(args.Arguments);
        Arguments.Add(argument);
        return new FFMpegArgumentProcessor(this);
    }

    private void MapAddedMetadata(List<IArgument> outputArguments)
    {
        if (outputArguments.Any(argument => argument is MapMetadataArgument or RemoveMetadataArgument))
        {
            return;
        }

        var inputIndex = 0;
        int? metadataInputIndex = null;
        foreach (var input in Arguments.OfType<IInputArgument>())
        {
            if (input is MetadataArgument or MetadataFileArgument)
            {
                metadataInputIndex = inputIndex;
            }

            inputIndex += input is MultiInputArgument multiInput ? multiInput.FilePaths.Count() : 1;
        }

        if (metadataInputIndex != null)
        {
            outputArguments.Insert(0, new MapMetadataArgument(metadataInputIndex.Value));
        }
    }

    public FFMpegArgumentProcessor OutputToTee(Action<TeeOutputOptions> addOutputs, Action<FFMpegOutputOptions>? addArguments = null)
    {
        var outputs = new TeeOutputOptions();
        addOutputs(outputs);
        return ToProcessor(new OutputTeeArgument(outputs), addArguments);
    }

    public FFMpegArgumentProcessor OutputToMany(Action<FFMpegMultiOutputOptions> addOutputs)
    {
        var args = new FFMpegMultiOutputOptions();
        addOutputs(args);
        foreach (var output in args.Outputs)
        {
            MapAddedMetadata(output.Arguments);
        }

        Arguments.AddRange(args.Arguments);
        return new FFMpegArgumentProcessor(this);
    }

    internal void Pre(FFOptions options)
    {
        foreach (var argument in Arguments.OfType<IInputOutputArgument>())
        {
            argument.Pre(options);
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
