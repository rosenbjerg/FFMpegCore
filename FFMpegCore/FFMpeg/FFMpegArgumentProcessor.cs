using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using Instances;

namespace FFMpegCore
{
    public class FFMpegArgumentProcessor
    {
        private static readonly Regex ProgressRegex = new Regex(@"time=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);
        private readonly List<Action<FFOptions>> _configurations;
        private readonly FFMpegArguments _ffMpegArguments;
        private Action<double>? _onPercentageProgress;
        private Action<TimeSpan>? _onTimeProgress;
        private Action<string, DataType>? _onOutput;
        private TimeSpan? _totalTimespan;

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
        public FFMpegArgumentProcessor NotifyOnOutput(Action<string, DataType> onOutput)
        {
            _onOutput = onOutput;
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
        public bool ProcessSynchronously(bool throwOnError = true, FFOptions? ffMpegOptions = null)
        {
            var options = GetConfiguredOptions(ffMpegOptions);
            using var instance = PrepareInstance(options, out var cancellationTokenSource);

            void OnCancelEvent(object sender, int timeout)
            {
                instance.SendInput("q");

                if (!cancellationTokenSource.Token.WaitHandle.WaitOne(timeout, true))
                {
                    cancellationTokenSource.Cancel();
                    instance.Started = false;
                }
            }
            CancelEvent += OnCancelEvent;
            instance.Exited += delegate { cancellationTokenSource.Cancel(); };

            var errorCode = -1;
            try
            {
                errorCode = Process(instance, cancellationTokenSource).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (!HandleException(throwOnError, e, instance.ErrorData)) return false;
            }
            finally
            {
                CancelEvent -= OnCancelEvent;
            }

            return HandleCompletion(throwOnError, errorCode, instance.ErrorData);
        }

        public async Task<bool> ProcessAsynchronously(bool throwOnError = true, FFOptions? ffMpegOptions = null)
        {
            var options = GetConfiguredOptions(ffMpegOptions);
            using var instance = PrepareInstance(options, out var cancellationTokenSource);

            void OnCancelEvent(object sender, int timeout)
            {
                instance.SendInput("q");

                if (!cancellationTokenSource.Token.WaitHandle.WaitOne(timeout, true))
                {
                    cancellationTokenSource.Cancel();
                    instance.Started = false;
                }
            }
            CancelEvent += OnCancelEvent;

            var errorCode = -1;
            try
            {
                errorCode = await Process(instance, cancellationTokenSource).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!HandleException(throwOnError, e, instance.ErrorData)) return false;
            }
            finally
            {
                CancelEvent -= OnCancelEvent;
            }

            return HandleCompletion(throwOnError, errorCode, instance.ErrorData);
        }

        private async Task<int> Process(Instance instance, CancellationTokenSource cancellationTokenSource)
        {
            var errorCode = -1;

            _ffMpegArguments.Pre();
            await Task.WhenAll(instance.FinishedRunning().ContinueWith(t =>
            {
                errorCode = t.Result;
                cancellationTokenSource.Cancel();
                _ffMpegArguments.Post();
            }), _ffMpegArguments.During(cancellationTokenSource.Token)).ConfigureAwait(false);

            return errorCode;
        }

        private bool HandleCompletion(bool throwOnError, int exitCode, IReadOnlyList<string> errorData)
        {
            if (throwOnError && exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffmpeg exited with non-zero exit-code ({exitCode} - {string.Join("\n", errorData)})", null, string.Join("\n", errorData));

            _onPercentageProgress?.Invoke(100.0);
            if (_totalTimespan.HasValue) _onTimeProgress?.Invoke(_totalTimespan.Value);

            return exitCode == 0;
        }

        private FFOptions GetConfiguredOptions(FFOptions? ffOptions)
        {
            var options = ffOptions ?? GlobalFFOptions.Current.Clone();

            foreach (var configureOptions in _configurations)
            {
                configureOptions(options);
            }

            return options;
        }

        private Instance PrepareInstance(FFOptions ffOptions,
            out CancellationTokenSource cancellationTokenSource)
        {
            FFMpegHelper.RootExceptionCheck();
            FFMpegHelper.VerifyFFMpegExists(ffOptions);
            var startInfo = new ProcessStartInfo
            {
                FileName = GlobalFFOptions.GetFFMpegBinaryPath(ffOptions),
                Arguments = _ffMpegArguments.Text,
                StandardOutputEncoding = ffOptions.Encoding,
                StandardErrorEncoding = ffOptions.Encoding,
                WorkingDirectory = ffOptions.WorkingDirectory
            };
            var instance = new Instance(startInfo);
            cancellationTokenSource = new CancellationTokenSource();

            if (_onOutput != null || _onTimeProgress != null || (_onPercentageProgress != null && _totalTimespan != null))
                instance.DataReceived += OutputData;

            return instance;
        }

        
        private static bool HandleException(bool throwOnError, Exception e, IReadOnlyList<string> errorData)
        {
            if (!throwOnError)
                return false;

            throw new FFMpegException(FFMpegExceptionType.Process, "Exception thrown during processing", e, string.Join("\n", errorData));
        }

        private void OutputData(object sender, (DataType Type, string Data) msg)
        {
            Debug.WriteLine(msg.Data);
            _onOutput?.Invoke(msg.Data, msg.Type);

            var match = ProgressRegex.Match(msg.Data);
            if (!match.Success) return;

            var processed = TimeSpan.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            _onTimeProgress?.Invoke(processed);

            if (_onPercentageProgress == null || _totalTimespan == null) return;
            var percentage = Math.Round(processed.TotalSeconds / _totalTimespan.Value.TotalSeconds * 100, 2);
            _onPercentageProgress(percentage);
        }
    }
}