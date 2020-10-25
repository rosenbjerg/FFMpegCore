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
        private TimeSpan? _totalTimespan;

        internal FFMpegArgumentProcessor(FFMpegArguments ffMpegArguments)
        {
            _ffMpegArguments = ffMpegArguments;
        }

        public string Arguments => _ffMpegArguments.Text;

        private event EventHandler CancelEvent = null!; 

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
        public FFMpegArgumentProcessor CancellableThrough(out Action cancel)
        {
            cancel = () => CancelEvent?.Invoke(this, EventArgs.Empty);
            return this;
        }
        public bool ProcessSynchronously(bool throwOnError = true)
        {
            using var instance = PrepareInstance(out var cancellationTokenSource);
            var errorCode = -1;

            void OnCancelEvent(object sender, EventArgs args)
            {
                instance?.SendInput("q");
                cancellationTokenSource.Cancel();
            }
            CancelEvent += OnCancelEvent;
            instance.Exited += delegate { cancellationTokenSource.Cancel(); };
            
            _ffMpegArguments.Pre();
            try
            {
                Task.WaitAll(instance.FinishedRunning().ContinueWith(t =>
                {
                    errorCode = t.Result;
                    cancellationTokenSource.Cancel();
                }), _ffMpegArguments.During(cancellationTokenSource.Token));
                _ffMpegArguments.Post();
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

        private bool HandleCompletion(bool throwOnError, int errorCode, IReadOnlyList<string> errorData)
        {
            if (throwOnError && errorCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Conversion, string.Join("\n", errorData));

            _onPercentageProgress?.Invoke(100.0);
            if (_totalTimespan.HasValue) _onTimeProgress?.Invoke(_totalTimespan.Value);

            return errorCode == 0;
        }

        public async Task<bool> ProcessAsynchronously(bool throwOnError = true)
        {
            using var instance = PrepareInstance(out var cancellationTokenSource);
            var errorCode = -1;

            void OnCancelEvent(object sender, EventArgs args)
            {
                instance?.SendInput("q");
                cancellationTokenSource.Cancel();
            }
            CancelEvent += OnCancelEvent;
            
            _ffMpegArguments.Pre();
            try
            {
                await Task.WhenAll(instance.FinishedRunning().ContinueWith(t =>
                {
                    errorCode = t.Result;
                    cancellationTokenSource.Cancel();
                }), _ffMpegArguments.During(cancellationTokenSource.Token)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!HandleException(throwOnError, e, instance.ErrorData)) return false;
            }
            finally
            {
                CancelEvent -= OnCancelEvent;
                _ffMpegArguments.Post();
            }

            return HandleCompletion(throwOnError, errorCode, instance.ErrorData);
        }

        private Instance PrepareInstance(out CancellationTokenSource cancellationTokenSource)
        {
            FFMpegHelper.RootExceptionCheck();
            FFMpegHelper.VerifyFFMpegExists();
            var instance = new Instance(FFMpegOptions.Options.FFmpegBinary(), _ffMpegArguments.Text);
            cancellationTokenSource = new CancellationTokenSource();

            if (_onTimeProgress != null || (_onPercentageProgress != null && _totalTimespan != null))
                instance.DataReceived += OutputData;

            return instance;
        }

        
        private static bool HandleException(bool throwOnError, Exception e, IReadOnlyList<string> errorData)
        {
            if (!throwOnError)
                return false;

            throw new FFMpegException(FFMpegExceptionType.Process, "Exception thrown during processing", e,
                string.Join("\n", errorData));
        }

        private void OutputData(object sender, (DataType Type, string Data) msg)
        {
            var match = ProgressRegex.Match(msg.Data);
            Debug.WriteLine(msg.Data);
            if (!match.Success) return;

            var processed = TimeSpan.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            _onTimeProgress?.Invoke(processed);

            if (_onPercentageProgress == null || _totalTimespan == null) return;
            var percentage = Math.Round(processed.TotalSeconds / _totalTimespan.Value.TotalSeconds * 100, 2);
            _onPercentageProgress(percentage);
        }
    }
}