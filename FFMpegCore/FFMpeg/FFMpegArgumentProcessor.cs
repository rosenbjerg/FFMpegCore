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
    private static readonly TimeSpan DefaultCancellationGracePeriod = TimeSpan.FromSeconds(5);
    private readonly List<(CancellationToken Token, TimeSpan GracePeriod)> _cancellationTokens = new();
    private bool _cancelled;
    private TimeSpan _cancellationGracePeriod;
    private FFOptions? _ffOptions;
    private TimeSpan? _knownDuration;
    private FFMpegLogLevel? _logLevel;
    private Action<string>? _onStandardError;
    private Action<string>? _onStandardOutput;
    private Action<double>? _onPercentageProgress;
    private Action<TimeSpan>? _onTimeProgress;
    private TimeSpan? _totalTimespan;

    internal FFMpegArgumentProcessor(FFMpegArguments ffMpegArguments)
    {
        _configurations = new List<Action<FFOptions>>();
        _ffMpegArguments = ffMpegArguments;
    }

    public string Arguments => _ffMpegArguments.Text;

    private event EventHandler<TimeSpan> CancelEvent = null!;

    /// <summary>
    ///     Register action that will be invoked during the ffmpeg processing, when a progress time is output and parsed and progress percentage is
    ///     calculated.
    ///     Total time is needed to calculate the percentage that has been processed of the full file.
    /// </summary>
    /// <param name="onPercentageProgress">Action to invoke when progress percentage is updated, with a value from 0 to 100</param>
    /// <param name="totalTimeSpan">The total timespan of the mediafile being processed</param>
    public FFMpegArgumentProcessor NotifyOnPercentageProgress(Action<double> onPercentageProgress, TimeSpan totalTimeSpan)
    {
        _totalTimespan = totalTimeSpan;
        _onPercentageProgress = onPercentageProgress;
        return this;
    }

    public FFMpegArgumentProcessor NotifyOnPercentageProgress(Action<double> onPercentageProgress)
    {
        if (_knownDuration == null)
        {
            throw new InvalidOperationException("The output duration is not known for these arguments; use the overload that takes the total duration");
        }

        return NotifyOnPercentageProgress(onPercentageProgress, _knownDuration.Value);
    }

    public FFMpegArgumentProcessor NotifyOnPercentageProgress(IProgress<double> percentageProgress, TimeSpan totalTimeSpan)
    {
        return NotifyOnPercentageProgress(percentageProgress.Report, totalTimeSpan);
    }

    public FFMpegArgumentProcessor NotifyOnPercentageProgress(IProgress<double> percentageProgress)
    {
        return NotifyOnPercentageProgress(percentageProgress.Report);
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

    public FFMpegArgumentProcessor NotifyOnProgress(IProgress<TimeSpan> timeProgress)
    {
        return NotifyOnProgress(timeProgress.Report);
    }

    internal FFMpegArgumentProcessor WithKnownDuration(TimeSpan duration)
    {
        _knownDuration = duration;
        return this;
    }

    public FFMpegArgumentProcessor NotifyOnStandardOutput(Action<string> onStandardOutput)
    {
        _onStandardOutput = onStandardOutput;
        return this;
    }

    public FFMpegArgumentProcessor NotifyOnStandardError(Action<string> onStandardError)
    {
        _onStandardError = onStandardError;
        return this;
    }

    private void Cancel(TimeSpan gracePeriod)
    {
        _cancelled = true;
        _cancellationGracePeriod = gracePeriod;
        CancelEvent?.Invoke(this, gracePeriod);
    }

    public FFMpegArgumentProcessor CancellableThrough(out Action cancel, TimeSpan? gracePeriod = null)
    {
        var resolvedGracePeriod = gracePeriod ?? DefaultCancellationGracePeriod;
        cancel = () => Cancel(resolvedGracePeriod);
        return this;
    }

    public FFMpegArgumentProcessor CancellableThrough(CancellationToken token, TimeSpan? gracePeriod = null)
    {
        token.ThrowIfCancellationRequested();
        _cancellationTokens.Add((token, gracePeriod ?? DefaultCancellationGracePeriod));
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

    public FFMpegResult ProcessSynchronously(bool throwOnError = true, FFOptions? ffOptions = null, CancellationToken cancellationToken = default)
    {
        var options = GetConfiguredOptions(ffOptions);
        using var cancellationTokenSource = new CancellationTokenSource();

        IProcessResult? processResult = null;
        var cancelled = false;
        try
        {
            processResult = Process(options, cancellationTokenSource, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
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

    public async Task<FFMpegResult> ProcessAsynchronously(bool throwOnError = true, FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)
    {
        var options = GetConfiguredOptions(ffOptions);
        using var cancellationTokenSource = new CancellationTokenSource();

        IProcessResult? processResult = null;
        var cancelled = false;
        try
        {
            processResult = await Process(options, cancellationTokenSource, cancellationToken).ConfigureAwait(false);
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

    private async Task<IProcessResult> Process(FFOptions options, CancellationTokenSource cancellationTokenSource, CancellationToken runToken)
    {
        IProcessResult processResult = null!;
        var tokens = runToken.CanBeCanceled
            ? _cancellationTokens.Append((Token: runToken, GracePeriod: DefaultCancellationGracePeriod)).ToList()
            : _cancellationTokens;
        if (_cancelled || tokens.Any(registered => registered.Token.IsCancellationRequested))
        {
            _cancelled = false;
            throw new OperationCanceledException("cancelled before starting processing");
        }

        FFMpegHelper.VerifyFFMpegExists(options);
        var registrations = tokens.Select(registered => registered.Token.Register(() => Cancel(registered.GracePeriod))).ToList();
        _ffMpegArguments.Pre(options);
        try
        {
            return await Run().ConfigureAwait(false);
        }
        finally
        {
            // Post() disposes what During() is still using; it runs once the run, and therefore During(), is over
            _ffMpegArguments.Post();
            foreach (var registration in registrations)
            {
                registration.Dispose();
            }

            _cancelled = false;
        }

        async Task<IProcessResult> Run()
        {
            using var instance = PrepareProcessArguments(options).Start();

            void OnCancelEvent(object sender, TimeSpan gracePeriod)
            {
                ExecuteIgnoringFinishedProcessExceptions(() => instance.SendInput("q"));

                // Don't wait for the grace period here: this runs inside the caller's CancellationTokenSource.Cancel()
                Task.Delay(gracePeriod, cancellationTokenSource.Token).ContinueWith(delay =>
                {
                    if (delay.IsCanceled)
                    {
                        return;
                    }

                    ExecuteIgnoringFinishedProcessExceptions(cancellationTokenSource.Cancel);
                    ExecuteIgnoringFinishedProcessExceptions(() => instance.Kill());
                }, TaskScheduler.Default);

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
            if (_cancelled)
            {
                OnCancelEvent(this, _cancellationGracePeriod);
            }

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
                    throw new OperationCanceledException("ffmpeg processing was cancelled");
                }

                return processResult;
            }
            finally
            {
                CancelEvent -= OnCancelEvent;
            }
        }
    }

    private FFMpegResult HandleCompletion(bool throwOnError, IProcessResult? processResult, bool cancelled)
    {
        var result = new FFMpegResult(processResult?.ExitCode ?? -1, processResult?.ErrorData ?? Array.Empty<string>(), cancelled);
        if (throwOnError && result.ExitCode != 0)
        {
            throw new FFMpegProcessException(result);
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

    internal FFMpegArgumentProcessor WithOptions(FFOptions? ffOptions)
    {
        _ffOptions = ffOptions;
        return this;
    }

    internal FFOptions GetConfiguredOptions(FFOptions? ffOptions)
    {
        var options = ffOptions ?? _ffOptions?.Clone() ?? GlobalFFOptions.Current.Clone();

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

        if (_onStandardOutput != null)
        {
            processArguments.OutputDataReceived += OutputData;
        }

        if (_onStandardError != null || reportsProgress)
        {
            processArguments.ErrorDataReceived += ErrorData;
        }

        return processArguments;
    }

    private void ErrorData(object sender, string msg)
    {
        _onStandardError?.Invoke(msg);

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
        _onStandardOutput?.Invoke(msg);
    }
}
