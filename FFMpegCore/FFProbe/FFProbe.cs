using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using FFMpegCore.Arguments;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using FFMpegCore.Pipes;
using Instances;

namespace FFMpegCore;

public static class FFProbe
{
    public static IMediaAnalysis Analyse(string filePath, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromFile(filePath, ffOptions, customArguments, PrepareStreamAnalysisInstance, result => ParseOutput(result, filePath));
    }

    public static IMediaAnalysis Analyse(Uri uri, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromUri(uri, ffOptions, customArguments, PrepareStreamAnalysisInstance, result => ParseOutput(result, uri.AbsoluteUri));
    }

    public static IMediaAnalysis Analyse(Stream stream, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromStream(stream, ffOptions, customArguments, PrepareStreamAnalysisInstance, result => ParseOutput(result, null));
    }

    public static Task<IMediaAnalysis> AnalyseAsync(string filePath, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromFileAsync(filePath, ffOptions, customArguments, PrepareStreamAnalysisInstance, result => ParseOutput(result, filePath),
            cancellationToken);
    }

    public static Task<IMediaAnalysis> AnalyseAsync(Uri uri, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromUriAsync(uri, ffOptions, customArguments, PrepareStreamAnalysisInstance, result => ParseOutput(result, uri.AbsoluteUri),
            cancellationToken);
    }

    public static Task<IMediaAnalysis> AnalyseAsync(Stream stream, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromStreamAsync(stream, ffOptions, customArguments, PrepareStreamAnalysisInstance, result => ParseOutput(result, null),
            cancellationToken);
    }

    public static IMediaAnalysis FromJson(string json)
    {
        return ParseAnalysis(json, null, Array.Empty<string>());
    }

    public static FFProbeFrames GetFrames(string filePath, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromFile(filePath, ffOptions, customArguments, PrepareFrameAnalysisInstance, ParseFramesOutput);
    }

    public static FFProbeFrames GetFrames(Uri uri, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromUri(uri, ffOptions, customArguments, PrepareFrameAnalysisInstance, ParseFramesOutput);
    }

    public static FFProbeFrames GetFrames(Stream stream, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromStream(stream, ffOptions, customArguments, PrepareFrameAnalysisInstance, ParseFramesOutput);
    }

    public static Task<FFProbeFrames> GetFramesAsync(string filePath, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromFileAsync(filePath, ffOptions, customArguments, PrepareFrameAnalysisInstance, ParseFramesOutput, cancellationToken);
    }

    public static Task<FFProbeFrames> GetFramesAsync(Uri uri, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromUriAsync(uri, ffOptions, customArguments, PrepareFrameAnalysisInstance, ParseFramesOutput, cancellationToken);
    }

    public static Task<FFProbeFrames> GetFramesAsync(Stream stream, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromStreamAsync(stream, ffOptions, customArguments, PrepareFrameAnalysisInstance, ParseFramesOutput, cancellationToken);
    }

    public static FFProbePackets GetPackets(string filePath, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromFile(filePath, ffOptions, customArguments, PreparePacketAnalysisInstance, ParsePacketsOutput);
    }

    public static FFProbePackets GetPackets(Uri uri, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromUri(uri, ffOptions, customArguments, PreparePacketAnalysisInstance, ParsePacketsOutput);
    }

    public static FFProbePackets GetPackets(Stream stream, FFOptions? ffOptions = null, string? customArguments = null)
    {
        return FromStream(stream, ffOptions, customArguments, PreparePacketAnalysisInstance, ParsePacketsOutput);
    }

    public static Task<FFProbePackets> GetPacketsAsync(string filePath, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromFileAsync(filePath, ffOptions, customArguments, PreparePacketAnalysisInstance, ParsePacketsOutput, cancellationToken);
    }

    public static Task<FFProbePackets> GetPacketsAsync(Uri uri, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromUriAsync(uri, ffOptions, customArguments, PreparePacketAnalysisInstance, ParsePacketsOutput, cancellationToken);
    }

    public static Task<FFProbePackets> GetPacketsAsync(Stream stream, FFOptions? ffOptions = null, string? customArguments = null,
        CancellationToken cancellationToken = default)
    {
        return FromStreamAsync(stream, ffOptions, customArguments, PreparePacketAnalysisInstance, ParsePacketsOutput, cancellationToken);
    }

    private delegate ProcessArguments PrepareProbe(string source, FFOptions ffOptions, string? customArguments);

    private static T FromFile<T>(string filePath, FFOptions? ffOptions, string? customArguments, PrepareProbe prepare, Func<IProcessResult, T> parse)
    {
        ThrowIfInputFileDoesNotExist(filePath);
        return Run(prepare(filePath, ffOptions ?? GlobalFFOptions.Current, customArguments), parse);
    }

    private static T FromUri<T>(Uri uri, FFOptions? ffOptions, string? customArguments, PrepareProbe prepare, Func<IProcessResult, T> parse)
    {
        return Run(prepare(uri.AbsoluteUri, ffOptions ?? GlobalFFOptions.Current, customArguments), parse);
    }

    private static T Run<T>(ProcessArguments processArguments, Func<IProcessResult, T> parse)
    {
        var result = processArguments.StartAndWaitForExit();
        ThrowIfExitCodeNotZero(result);
        return parse(result);
    }

    private static async Task<T> FromFileAsync<T>(string filePath, FFOptions? ffOptions, string? customArguments, PrepareProbe prepare,
        Func<IProcessResult, T> parse, CancellationToken cancellationToken)
    {
        ThrowIfInputFileDoesNotExist(filePath);
        return await RunAsync(prepare(filePath, ffOptions ?? GlobalFFOptions.Current, customArguments), parse, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<T> FromUriAsync<T>(Uri uri, FFOptions? ffOptions, string? customArguments, PrepareProbe prepare,
        Func<IProcessResult, T> parse, CancellationToken cancellationToken)
    {
        return await RunAsync(prepare(uri.AbsoluteUri, ffOptions ?? GlobalFFOptions.Current, customArguments), parse, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<T> RunAsync<T>(ProcessArguments processArguments, Func<IProcessResult, T> parse, CancellationToken cancellationToken)
    {
        var result = await processArguments.StartAndWaitForExitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfExitCodeNotZero(result);
        return parse(result);
    }

    private static T FromStream<T>(Stream stream, FFOptions? ffOptions, string? customArguments, PrepareProbe prepare, Func<IProcessResult, T> parse)
    {
        return FromStreamAsync(stream, ffOptions, customArguments, prepare, parse, CancellationToken.None).ConfigureAwait(false).GetAwaiter()
            .GetResult();
    }

    private static async Task<T> FromStreamAsync<T>(Stream stream, FFOptions? ffOptions, string? customArguments, PrepareProbe prepare,
        Func<IProcessResult, T> parse, CancellationToken cancellationToken)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        var pipeArgument = new InputPipeArgument(new StreamPipeSource(stream));
        var processArguments = prepare(pipeArgument.PipePath, options, customArguments);
        pipeArgument.Pre(options);

        var exitedOrCancelled = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var task = processArguments.StartAndWaitForExitAsync(cancellationToken);
        var exit = task.ContinueWith(_ => exitedOrCancelled.Cancel(), TaskScheduler.Default);
        try
        {
            await pipeArgument.During(exitedOrCancelled.Token).ConfigureAwait(false);
        }
        catch (IOException) { }
        finally
        {
            pipeArgument.Post();
            await exit.ConfigureAwait(false);
            exitedOrCancelled.Dispose();
        }

        var result = await task.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfExitCodeNotZero(result);
        return parse(result);
    }

    private static IMediaAnalysis ParseOutput(IProcessResult instance, string? path)
    {
        return ParseAnalysis(string.Join("\n", instance.OutputData), path, instance.ErrorData);
    }

    private static IMediaAnalysis ParseAnalysis(string json, string? path, IReadOnlyList<string> standardError)
    {
        var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbeAnalysis>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (ffprobeAnalysis?.Format == null)
        {
            throw new FormatNullException();
        }

        ffprobeAnalysis.StandardError = standardError;
        return new MediaAnalysis(ffprobeAnalysis, path, json);
    }

    private static FFProbeFrames ParseFramesOutput(IProcessResult instance)
    {
        var json = string.Join(string.Empty, instance.OutputData);
        var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbeFrames>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            });

        return ffprobeAnalysis!;
    }

    private static FFProbePackets ParsePacketsOutput(IProcessResult instance)
    {
        var json = string.Join(string.Empty, instance.OutputData);
        var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbePackets>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            });

        return ffprobeAnalysis!;
    }

    private static void ThrowIfInputFileDoesNotExist(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FFProbeException(FFMpegExceptionType.File, $"No file found at '{filePath}'");
        }
    }

    private static void ThrowIfExitCodeNotZero(IProcessResult result)
    {
        if (result.ExitCode != 0)
        {
            throw new FFProbeProcessException(result.ExitCode, result.ErrorData);
        }
    }

    private static ProcessArguments PrepareStreamAnalysisInstance(string filePath, FFOptions ffOptions, string? customArguments)
    {
        return PrepareInstance($"-loglevel error -print_format json -show_format -sexagesimal -show_streams -show_chapters \"{filePath}\"", ffOptions,
            customArguments);
    }

    private static ProcessArguments PrepareFrameAnalysisInstance(string filePath, FFOptions ffOptions, string? customArguments)
    {
        return PrepareInstance($"-loglevel error -print_format json -show_frames -sexagesimal \"{filePath}\"", ffOptions, customArguments);
    }

    private static ProcessArguments PreparePacketAnalysisInstance(string filePath, FFOptions ffOptions, string? customArguments)
    {
        return PrepareInstance($"-loglevel error -print_format json -show_packets -sexagesimal \"{filePath}\"", ffOptions, customArguments);
    }

    private static ProcessArguments PrepareInstance(string arguments, FFOptions ffOptions, string? customArguments)
    {
        FFProbeHelper.VerifyFFProbeExists(ffOptions);
        var startInfo = new ProcessStartInfo(GlobalFFOptions.GetFFProbeBinaryPath(ffOptions), $"{arguments} {customArguments}")
        {
            StandardOutputEncoding = ffOptions.Encoding,
            StandardErrorEncoding = ffOptions.Encoding,
            WorkingDirectory = ffOptions.WorkingDirectory
        };
        return new ProcessArguments(startInfo);
    }
}
