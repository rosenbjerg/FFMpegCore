using FFMpegCore.Arguments;
using FFMpegCore.Pipes;

namespace FFMpegCore;

public sealed class FFMpegArguments : FFMpegArgumentsBase
{
    private FFMpegArguments() { }

    public string Text => GetText();

    private string GetText()
    {
        var allArguments = Arguments.ToArray();
        return string.Join(" ", allArguments
            .Select(arg => arg is IDynamicArgument dynArg ? dynArg.GetText(allArguments) : arg.Text)
            .Where(text => !string.IsNullOrEmpty(text)));
    }

    public static FFMpegArguments FromConcatInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new ConcatArgument(filePaths), addArguments);
    }

    public static FFMpegArguments FromDemuxConcatInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new DemuxConcatArgument(filePaths), addArguments);
    }

    public static FFMpegArguments FromFileInput(string filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(verifyExists, filePath), addArguments);
    }

    public static FFMpegArguments FromFileInput(IEnumerable<string> filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new MultiInputArgument(verifyExists, filePath), addArguments);
    }

    public static FFMpegArguments FromFileInput(FileInfo fileInfo, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(fileInfo.FullName, false), addArguments);
    }

    public static FFMpegArguments FromUrlInput(Uri uri, Action<FFMpegInputOptions>? addArguments = null)
    {
        return new FFMpegArguments().WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
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

    public FFMpegArguments AddConcatInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new ConcatArgument(filePaths), addArguments);
    }

    public FFMpegArguments AddDemuxConcatInput(IEnumerable<string> filePaths, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new DemuxConcatArgument(filePaths), addArguments);
    }

    public FFMpegArguments AddFileInput(string filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(verifyExists, filePath), addArguments);
    }

    public FFMpegArguments AddFileInput(IEnumerable<string> filePath, bool verifyExists = true, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new MultiInputArgument(verifyExists, filePath), addArguments);
    }

    public FFMpegArguments AddFileInput(FileInfo fileInfo, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(fileInfo.FullName, false), addArguments);
    }

    public FFMpegArguments AddUrlInput(Uri uri, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new InputArgument(uri.AbsoluteUri, false), addArguments);
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

    public FFMpegArguments AddMetadata(string content, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new MetaDataArgument(content), addArguments);
    }

    public FFMpegArguments AddMetadata(FFMetadataBuilder metaDataBuilder, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new MetaDataArgument(metaDataBuilder.Build()), addArguments);
    }

    /// <summary>
    ///     Maps the metadata of the given stream
    /// </summary>
    /// <param name="inputIndex">null means, the previous input will be used</param>
    public FFMpegArguments MapMetadata(int? inputIndex = null, Action<FFMpegInputOptions>? addArguments = null)
    {
        return WithInput(new MapMetadataArgument(inputIndex), addArguments);
    }

    private FFMpegArguments WithInput(IInputArgument inputArgument, Action<FFMpegInputOptions>? addArguments)
    {
        var arguments = new FFMpegInputOptions();
        addArguments?.Invoke(arguments);
        Arguments.AddRange(arguments.Arguments);
        Arguments.Add(inputArgument);
        return this;
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
        return ToProcessor(new OutputUrlArgument(uri.ToString()), addArguments);
    }

    public FFMpegArgumentProcessor OutputToPipe(IPipeSink reader, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return ToProcessor(new OutputPipeArgument(reader), addArguments);
    }

    private FFMpegArgumentProcessor ToProcessor(IOutputArgument argument, Action<FFMpegOutputOptions>? addArguments)
    {
        var args = new FFMpegOutputOptions();
        addArguments?.Invoke(args);
        Arguments.AddRange(args.Arguments);
        Arguments.Add(argument);
        return new FFMpegArgumentProcessor(this);
    }

    public FFMpegArgumentProcessor OutputToTee(Action<FFMpegMultiOutputOptions> addOutputs, Action<FFMpegOutputOptions>? addArguments = null)
    {
        var outputs = new FFMpegMultiOutputOptions();
        addOutputs(outputs);
        return ToProcessor(new OutputTeeArgument(outputs), addArguments);
    }

    public FFMpegArgumentProcessor OutputToMany(Action<FFMpegMultiOutputOptions> addOutputs)
    {
        var args = new FFMpegMultiOutputOptions();
        addOutputs(args);
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
