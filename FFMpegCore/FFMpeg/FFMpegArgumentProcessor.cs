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
        private readonly FFMpegArguments _ffMpegArguments;
        private Action<double>? _onPercentageProgress;
        private Action<TimeSpan>? _onTimeProgress;
        private Action<string, DataType>? _onOutput;
        private TimeSpan? _totalTimespan;

        internal FFMpegArgumentProcessor(FFMpegArguments ffMpegArguments)
        {
            _ffMpegArguments = ffMpegArguments;
        }

        public string Arguments => _ffMpegArguments.Text;

        private event EventHandler<int> CancelEvent = null!; 

        public FFMpegArgumentProcessor NotifyOnProgress(Action<double> onPercentageProgress, TimeSpan totalTimeSpan)
        {
            _totalTimespan = totalTimeSpan;
            _onPercentageProgress = onPercentageProgress;
            return this;
        }
        public FFMpegArgumentProcessor NotifyOnProgress(Action<TimeSpan> onTimeProgress)
        {
            _onTimeProgress = onTimeProgress;
            return this;
        }
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
        public bool ProcessSynchronously(bool throwOnError = true)
        {
            using var instance = PrepareInstance(out var cancellationTokenSource);
            var errorCode = -1;

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
            
            try
            {
                _ffMpegArguments.Pre();
                Task.WaitAll(instance.FinishedRunning().ContinueWith(t =>
                {
                    errorCode = t.Result;
                    cancellationTokenSource.Cancel();
                    _ffMpegArguments.Post();
                }), _ffMpegArguments.During(cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                if (!HandleException(throwOnError, e, instance.ErrorData, instance.OutputData)) return false;
            }
            finally
            {
                CancelEvent -= OnCancelEvent;
            }
            
            return HandleCompletion(throwOnError, errorCode, instance.ErrorData);
        }

        private bool HandleCompletion(bool throwOnError, int exitCode, IReadOnlyList<string> errorData)
        {
            if (throwOnError && exitCode != 0)
                throw new FFMpegProcessException(exitCode, string.Join("\n", errorData));

            _onPercentageProgress?.Invoke(100.0);
            if (_totalTimespan.HasValue) _onTimeProgress?.Invoke(_totalTimespan.Value);

            return exitCode == 0;
        }

        public async Task<bool> ProcessAsynchronously(bool throwOnError = true)
        {
            using var instance = PrepareInstance(out var cancellationTokenSource);
            var errorCode = -1;

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
            
            try
            {
                _ffMpegArguments.Pre();
                await Task.WhenAll(instance.FinishedRunning().ContinueWith(t =>
                {
                    errorCode = t.Result;
                    cancellationTokenSource.Cancel();
                    _ffMpegArguments.Post();
                }), _ffMpegArguments.During(cancellationTokenSource.Token)).ConfigureAwait(false);
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

        private Instance PrepareInstance(out CancellationTokenSource cancellationTokenSource)
        {
            FFMpegHelper.RootExceptionCheck();
            FFMpegHelper.VerifyFFMpegExists();
            var startInfo = new ProcessStartInfo
            {
                FileName = FFMpegOptions.Options.FFMpegBinary(),
                Arguments = _ffMpegArguments.Text,
                StandardOutputEncoding = FFMpegOptions.Options.Encoding,
                StandardErrorEncoding = FFMpegOptions.Options.Encoding,
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

            throw new FFMpegProcessException(exitCode, string.Join("\n", errorData));
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