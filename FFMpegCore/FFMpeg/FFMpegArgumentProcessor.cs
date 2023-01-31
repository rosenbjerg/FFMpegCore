using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using Instances;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Enums;

namespace FFMpegCore
{
    public class FFMpegArgumentProcessor
    {
        private static readonly Regex ProgressRegex = new Regex(@"time=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);
        private readonly List<Action<FFOptions>> _configurations;
        private readonly FFMpegArguments _ffMpegArguments;
        private Action<double>? _onPercentageProgress;
        private Action<TimeSpan>? _onTimeProgress;
        private Action<string>? _onOutput;
        private Action<string>? _onError;
        private TimeSpan? _totalTimespan;
        private FFMpegLogLevel? _logLevel;

        internal FFMpegArgumentProcessor(FFMpegArguments ffMpegArguments)
        {
            _configurations = new List<Action<FFOptions>>();
            _ffMpegArguments = ffMpegArguments;
        }

        public string Arguments => _ffMpegArguments.Text;

        private event EventHandler<int> CancelEvent = null!;

        /// <summary>
        /// Register action that will be invoked during the ffmpeg processing, when a progress time is output and parsed and progress percentage is calculated.
        /// Total time is needed to calculate the percentage that has been processed of the full file.
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
        /// Register action that will be invoked during the ffmpeg processing, when a progress time is output and parsed
        /// </summary>
        /// <param name="onTimeProgress">Action that will be invoked with the parsed timestamp as argument</param>
        public FFMpegArgumentProcessor NotifyOnProgress(Action<TimeSpan> onTimeProgress)
        {
            _onTimeProgress = onTimeProgress;
            return this;
        }

        /// <summary>
        /// Register action that will be invoked during the ffmpeg processing, when a line is output
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
        public FFMpegArgumentProcessor CancellableThrough(out Action cancel, int timeout = 0)
        {
            cancel = () => CancelEvent?.Invoke(this, timeout);
            return this;
        }
        public FFMpegArgumentProcessor CancellableThrough(CancellationToken token, int timeout = 0)
        {
            token.Register(() => CancelEvent?.Invoke(this, timeout));
            return this;
        }
        public FFMpegArgumentProcessor Configure(Action<FFOptions> configureOptions)
        {
            _configurations.Add(configureOptions);
            return this;
        }

        /// <summary>
        /// Sets the log level of this process. Overides the <see cref="FFMpegLogLevel"/>
        /// that is set in the <see cref="FFOptions"/> for this specific process.
        /// </summary>
        /// <param name="logLevel">The log level of the ffmpeg execution.</param>
        public FFMpegArgumentProcessor WithLogLevel(FFMpegLogLevel logLevel)
        {
            _logLevel = logLevel;
            return this;
        }

        public bool ProcessSynchronously(bool throwOnError = true, FFOptions? ffMpegOptions = null)
        {
            var options = GetConfiguredOptions(ffMpegOptions);
            var processArguments = PrepareProcessArguments(options, out var cancellationTokenSource);

            IProcessResult? processResult = null;
            try
            {
                processResult = Process(processArguments, cancellationTokenSource).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                if (throwOnError)
                    throw;
            }

            return HandleCompletion(throwOnError, processResult?.ExitCode ?? -1, processResult?.ErrorData ?? Array.Empty<string>());
        }

        public async Task<bool> ProcessAsynchronously(bool throwOnError = true, FFOptions? ffMpegOptions = null)
        {
            var options = GetConfiguredOptions(ffMpegOptions);
            var processArguments = PrepareProcessArguments(options, out var cancellationTokenSource);

            IProcessResult? processResult = null;
            try
            {
                processResult = await Process(processArguments, cancellationTokenSource).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (throwOnError)
                    throw;
            }

            return HandleCompletion(throwOnError, processResult?.ExitCode ?? -1, processResult?.ErrorData ?? Array.Empty<string>());
        }

        private async Task<IProcessResult> Process(ProcessArguments processArguments, CancellationTokenSource cancellationTokenSource)
        {
            IProcessResult processResult = null!;

            _ffMpegArguments.Pre();

            using var instance = processArguments.Start();
            var cancelled = false;
            void OnCancelEvent(object sender, int timeout)
            {
                cancelled = true;
                instance.SendInput("q");

                if (!cancellationTokenSource.Token.WaitHandle.WaitOne(timeout, true))
                {
                    cancellationTokenSource.Cancel();
                    instance.Kill();
                }
            }
            CancelEvent += OnCancelEvent;

            try
            {
                await Task.WhenAll(instance.WaitForExitAsync().ContinueWith(t =>
                {
                    processResult = t.Result;
                    cancellationTokenSource.Cancel();
                    _ffMpegArguments.Post();
                }), _ffMpegArguments.During(cancellationTokenSource.Token)).ConfigureAwait(false);

                if (cancelled)
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

        private bool HandleCompletion(bool throwOnError, int exitCode, IReadOnlyList<string> errorData)
        {
            if (throwOnError && exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffmpeg exited with non-zero exit-code ({exitCode} - {string.Join("\n", errorData)})", null, string.Join("\n", errorData));

            _onPercentageProgress?.Invoke(100.0);
            if (_totalTimespan.HasValue) _onTimeProgress?.Invoke(_totalTimespan.Value);

            return exitCode == 0;
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

        private ProcessArguments PrepareProcessArguments(FFOptions ffOptions,
            out CancellationTokenSource cancellationTokenSource)
        {
            FFMpegHelper.RootExceptionCheck();
            FFMpegHelper.VerifyFFMpegExists(ffOptions);

            string? arguments = _ffMpegArguments.Text;

            //If local loglevel is null, set the global.
            if (_logLevel == null)
                _logLevel = ffOptions.LogLevel;

            //If neither local nor global loglevel is null, set the argument.
            if (_logLevel != null)
            {
                string normalizedLogLevel = _logLevel.ToString()
                                                     .ToLower();
                arguments += $" -v {normalizedLogLevel}";
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
            cancellationTokenSource = new CancellationTokenSource();

            if (_onOutput != null)
                processArguments.OutputDataReceived += OutputData;

            if (_onError != null || _onTimeProgress != null || (_onPercentageProgress != null && _totalTimespan != null))
                processArguments.ErrorDataReceived += ErrorData;

            return processArguments;
        }

        private void ErrorData(object sender, string msg)
        {
            _onError?.Invoke(msg);

            var match = ProgressRegex.Match(msg);
            if (!match.Success) return;

            var processed = TimeSpan.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            _onTimeProgress?.Invoke(processed);

            if (_onPercentageProgress == null || _totalTimespan == null) return;
            var percentage = Math.Round(processed.TotalSeconds / _totalTimespan.Value.TotalSeconds * 100, 2);
            _onPercentageProgress(percentage);
        }

        private void OutputData(object sender, string msg)
        {
            Debug.WriteLine(msg);
            _onOutput?.Invoke(msg);
        }
    }
}