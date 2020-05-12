using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Helpers;
using FFMpegCore.Models;
using Instances;

namespace FFMpegCore
{
    public class FFMpegArgumentProcessor
    {
        private readonly FFMpegArguments _ffMpegArguments;

        internal FFMpegArgumentProcessor(FFMpegArguments ffMpegArguments)
        {
            _ffMpegArguments = ffMpegArguments;
        }

        /// <summary>
        /// Returns the percentage of the current conversion progress.
        /// </summary>
        // public event ConversionHandler OnProgress;

        public string Arguments => _ffMpegArguments.Text;

        public FFMpegArgumentProcessor NotifyOnProgress(Action<double>? onPercentageProgress, TimeSpan? totalTimeSpan)
        {
            _totalTimespan = totalTimeSpan;
            _onPercentageProgress = onPercentageProgress;
            return this;
        }
        public FFMpegArgumentProcessor NotifyOnProgress(Action<TimeSpan>? onTimeProgress)
        {
            _onTimeProgress = onTimeProgress;
            return this;
        }
        public bool ProcessSynchronously(bool throwOnError = true)
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);
            using var instance = new Instance(FFMpegOptions.Options.FFmpegBinary(), _ffMpegArguments.Text);
            instance.DataReceived += OutputData;
            var errorCode = -1;

            _ffMpegArguments.Pre();
            
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                Task.WaitAll(instance.FinishedRunning().ContinueWith(t =>
                {
                    errorCode = t.Result;
                    cancellationTokenSource.Cancel();
                }), _ffMpegArguments.During(cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                if (!throwOnError) 
                    return false;
                
                throw new FFMpegException(FFMpegExceptionType.Process, "Exception thrown during processing", e,
                    string.Join("\n", instance.ErrorData));
            }
            finally
            {
                _ffMpegArguments.Post();
            }
            
            if (throwOnError && errorCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Conversion, string.Join("\n", instance.ErrorData));
            
            return errorCode == 0;
        }
        
        public async Task<bool> ProcessAsynchronously(bool throwOnError = true)
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);
            using var instance = new Instance(FFMpegOptions.Options.FFmpegBinary(), _ffMpegArguments.Text);
            if (_onTimeProgress != null || (_onPercentageProgress != null && _totalTimespan != null))
                instance.DataReceived += OutputData;
            var errorCode = -1;

            _ffMpegArguments.Pre();
            
            var cancellationTokenSource = new CancellationTokenSource();
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
                if (!throwOnError) 
                    return false;

                throw new FFMpegException(FFMpegExceptionType.Process, "Exception thrown during processing", e,
                    string.Join("\n", instance.ErrorData));
            }
            finally
            {
                _ffMpegArguments.Post();
            }
            
            if (throwOnError && errorCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Conversion, string.Join("\n", instance.ErrorData));
            
            return errorCode == 0;
        }
        
        
        private static readonly Regex ProgressRegex = new Regex(@"time=(\d\d:\d\d:\d\d.\d\d?)", RegexOptions.Compiled);
        private Action<double>? _onPercentageProgress;
        private Action<TimeSpan>? _onTimeProgress;
        private TimeSpan? _totalTimespan;

        private void OutputData(object sender, (DataType Type, string Data) msg)
        {
#if DEBUG
            Trace.WriteLine(msg.Data);
#endif

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