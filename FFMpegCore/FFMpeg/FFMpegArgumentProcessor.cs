using System.Diagnostics;
using System.Text.RegularExpressions;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using Instances;

namespace FFMpegCore;

public class FFMpegArgumentProcessor
{
    private static readonly Regex ProgressRegex = new(@"time=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);
    private readonly List<Action<FFOptions>> _configurations;
    private readonly FFMpegArguments _ffMpegArguments;
    private readonly List<CancellationTokenRegistration> _cancellationTokenRegistrations = new();
    private bool _cancelled;
    private FFMpegLogLevel? _logLevel;
    private Action<string>? _onError;
    private Action<string>? _onOutput;
    private Action<double>? _onPercentageProgress;
    private Action<TimeSpan>? _onTimeProgress;
    private TimeSpan? _totalTimespan;

    internal FFMpegArgumentProcessor(FFMpegArguments ffMpegArguments)
    {
        _configurations = new List<Action<FFOptions>>();
        _ffMpegArguments = ffMpegArguments;
    }

    public string Arguments => _ffMpegArguments.Text;

    private event EventHandler<int> CancelEvent = null!;

    /// <summary>
    ///     Register action that will be invoked during the ffmpeg processing, when a progress time is output and parsed and progress percentage is
    ///     calculated.
    ///     Total time is needed to calculate the percentage that has been processed of the full file.
    /// </summary>
    /// <param name="onPercentageProgress">Action to invoke when progress percentage is updated</param>
    /// <param name="totalTimeSpan">The total timespan of the mediafile being processed</param>
    public FFMpegArgumentProcessor NotifyOnProgress(Action<double> onPercentageProgress, TimeSpan totalTimeSpan)
    {
        _totalTimespan = totalTimeSpan;
        _onPercentageProgress = onPercentageProgress;
        return this;
    }

    /// <summary>
    ///     Register action that will be invoked during the ffmpeg processing, when a progress time is output and parsed
    /// </summary>
    /// <param name="onTimeProgress">Action that will be invoked with the parsed timestamp as argument</param>
    public FFMpegArgumentProcessor NotifyOnProgress(Action<TimeSpan> onTimeProgress)
    {
        _onTimeProgress = onTimeProgress;
        return this;
    }

    /// <summary>
    ///     Register action that will be invoked during the ffmpeg processing, when a line is output
    /// </summary>
    /// <param name="onOutput"></param>
    public FFMpegArgumentProcessor NotifyOnOutput(Action<string> onOutput)
    {
        _onOutput = onOutput;
        return this;
    }

    public FFMpegArgumentProcessor NotifyOnError(Action<string> onError)
    {
        _onError = onError;
        return this;
    }

    private void Cancel(int timeout)
    {
        _cancelled = true;
        CancelEvent?.Invoke(this, timeout);
    }

    public FFMpegArgumentProcessor CancellableThrough(out Action cancel, int timeout = 0)
    {
        cancel = () => Cancel(timeout);
        return this;
    }

    public FFMpegArgumentProcessor CancellableThrough(CancellationToken token, int timeout = 0)
    {
        token.ThrowIfCancellationRequested();
        _cancellationTokenRegistrations.Add(token.Register(() => Cancel(timeout)));
        return this;
    }

    public FFMpegArgumentProcessor Configure(Action<FFOptions> configureOptions)
    {
        _configurations.Add(configureOptions);
        return this;
    }

    /// <summary>
    ///     Sets the log level of this process. Overides the <see cref="FFMpegLogLevel" />
    ///     that is set in the <see cref="FFOptions" /> for this specific process.
    /// </summary>
    /// <param name="logLevel">The log level of the ffmpeg execution.</param>
    public FFMpegArgumentProcessor WithLogLevel(FFMpegLogLevel logLevel)
    {
        _logLevel = logLevel;
        return this;
    }

    public FFMpegResult ProcessSynchronously(bool throwOnError = true, FFOptions? ffMpegOptions = null)
    {
        var options = GetConfiguredOptions(ffMpegOptions);
        using var cancellationTokenSource = new CancellationTokenSource();

        IProcessResult? processResult = null;
        var cancelled = false;
        try
        {
            processResult = Process(options, cancellationTokenSource).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            if (throwOnError)
            {
                throw;
            }

            cancelled = true;
        }

        return HandleCompletion(throwOnError, processResult, cancelled);
    }

    public async Task<FFMpegResult> ProcessAsynchronously(bool throwOnError = true, FFOptions? ffMpegOptions = null)
    {
        var options = GetConfiguredOptions(ffMpegOptions);
        using var cancellationTokenSource = new CancellationTokenSource();

        IProcessResult? processResult = null;
        var cancelled = false;
        try
        {
            processResult = await Process(options, cancellationTokenSource).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (throwOnError)
            {
                throw;
            }

            cancelled = true;
        }

        return HandleCompletion(throwOnError, processResult, cancelled);
    }

    private async Task<IProcessResult> Process(FFOptions options, CancellationTokenSource cancellationTokenSource)
    {
        IProcessResult processResult = null!;
        if (_cancelled)
        {
            DisposeCancellationRegistrations();
            throw new OperationCanceledException("cancelled before starting processing");
        }

        FFMpegHelper.VerifyFFMpegExists(options);
        _ffMpegArguments.Pre(options);
        try
        {
            return await Run().ConfigureAwait(false);
        }
        finally
        {
            // Post() disposes what During() is still using; it runs once the run, and therefore During(), is over
            _ffMpegArguments.Post();
        }

        async Task<IProcessResult> Run()
        {
            using var instance = PrepareProcessArguments(options).Start();

            void OnCancelEvent(object sender, int timeout)
            {
                ExecuteIgnoringFinishedProcessExceptions(() => instance.SendInput("q"));

                if (!cancellationTokenSource.Token.WaitHandle.WaitOne(timeout, true))
                {
                    cancellationTokenSource.Cancel();
                    ExecuteIgnoringFinishedProcessExceptions(() => instance.Kill());
                }

                static void ExecuteIgnoringFinishedProcessExceptions(Action action)
                {
                    try
                    {
                        action();
                    }
                    catch (Instances.Exceptions.InstanceProcessAlreadyExitedException)
                    {
                        //ignore
                    }
                    catch (ObjectDisposedException)
                    {
                        //ignore
                    }
                }
            }

            CancelEvent += OnCancelEvent;

            try
            {
                var during = _ffMpegArguments.During(cancellationTokenSource.Token);
                var exit = instance.WaitForExitAsync().ContinueWith(t =>
                {
                    processResult = t.Result;
                    cancellationTokenSource.Cancel();
                });

                try
                {
                    await Task.WhenAll(exit, during).ConfigureAwait(false);
                }
                catch (Exception) when (exit.Status == TaskStatus.RanToCompletion && processResult.ExitCode != 0)
                {
                    // ffmpeg failed; its exit code and stderr are the error, not the pipe it left broken
                }

                if (_cancelled)
                {
                    DisposeCancellationRegistrations();
                    throw new OperationCanceledException("ffmpeg processing was cancelled");
                }

                return processResult;
            }
            finally
            {
                CancelEvent -= OnCancelEvent;
                DisposeCancellationRegistrations();
            }
        }
    }

    private void DisposeCancellationRegistrations()
    {
        foreach (var registration in _cancellationTokenRegistrations)
        {
            registration.Dispose();
        }

        _cancellationTokenRegistrations.Clear();
    }

    private FFMpegResult HandleCompletion(bool throwOnError, IProcessResult? processResult, bool cancelled)
    {
        var result = new FFMpegResult(processResult?.ExitCode ?? -1, processResult?.ErrorData ?? Array.Empty<string>(), cancelled);
        if (throwOnError && result.ExitCode != 0)
        {
            var errorOutput = string.Join("\n", result.ErrorOutput);
            throw new FFMpegException(FFMpegExceptionType.Process, $"ffmpeg exited with non-zero exit-code ({result.ExitCode} - {errorOutput})", null, errorOutput);
        }

        if (result.Success)
        {
            _onPercentageProgress?.Invoke(100.0);
            if (_totalTimespan.HasValue)
            {
                _onTimeProgress?.Invoke(_totalTimespan.Value);
            }
        }

        return result;
    }

    internal FFOptions GetConfiguredOptions(FFOptions? ffOptions)
    {
        var options = ffOptions ?? GlobalFFOptions.Current.Clone();

        foreach (var configureOptions in _configurations)
        {
            configureOptions(options);
        }

        return options;
    }

    private ProcessArguments PrepareProcessArguments(FFOptions ffOptions)
    {
        var arguments = _ffMpegArguments.Text;

        var logLevel = _logLevel ?? ffOptions.LogLevel;
        if (logLevel != null)
        {
            arguments += $" -v {logLevel.ToString().ToLower()}";
        }

        var reportsProgress = _onTimeProgress != null || (_onPercentageProgress != null && _totalTimespan != null);
        if (reportsProgress)
        {
            arguments += " -stats";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = GlobalFFOptions.GetFFMpegBinaryPath(ffOptions),
            Arguments = arguments,
            StandardOutputEncoding = ffOptions.Encoding,
            StandardErrorEncoding = ffOptions.Encoding,
            WorkingDirectory = ffOptions.WorkingDirectory
        };
        var processArguments = new ProcessArguments(startInfo);

        if (_onOutput != null)
        {
            processArguments.OutputDataReceived += OutputData;
        }

        if (_onError != null || reportsProgress)
        {
            processArguments.ErrorDataReceived += ErrorData;
        }

        return processArguments;
    }

    private void ErrorData(object sender, string msg)
    {
        _onError?.Invoke(msg);

        var match = ProgressRegex.Match(msg);
        if (!match.Success)
        {
            return;
        }

        var processed = MediaAnalysisUtils.ParseDuration(match.Groups[1].Value);
        _onTimeProgress?.Invoke(processed);

        if (_onPercentageProgress == null || _totalTimespan == null)
        {
            return;
        }

        var percentage = Math.Round(processed.TotalSeconds / _totalTimespan.Value.TotalSeconds * 100, 2);
        _onPercentageProgress(percentage);
    }

    private void OutputData(object sender, string msg)
    {
        Debug.WriteLine(msg);
        _onOutput?.Invoke(msg);
    }
}
