using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FFMpegCore.Arguments;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using FFMpegCore.Pipes;
using Instances;

namespace FFMpegCore
{
    public static class FFProbe
    {
        public static IMediaAnalysis Analyse(string filePath, int outputCapacity = int.MaxValue, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath)) 
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");
            
            using var instance = PrepareInstance(filePath, outputCapacity, ffOptions ?? GlobalFFOptions.Current);
            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({exitCode} - {string.Join("\n", instance.ErrorData)})", null, string.Join("\n", instance.ErrorData));
            
            return ParseOutput(instance);
        }
        public static IMediaAnalysis Analyse(Uri uri, int outputCapacity = int.MaxValue, FFOptions? ffOptions = null)
        {
            using var instance = PrepareInstance(uri.AbsoluteUri, outputCapacity, ffOptions ?? GlobalFFOptions.Current);
            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({exitCode} - {string.Join("\n", instance.ErrorData)})", null, string.Join("\n", instance.ErrorData));

            return ParseOutput(instance);
        }
        public static IMediaAnalysis Analyse(Stream stream, int outputCapacity = int.MaxValue, FFOptions? ffOptions = null)
        {
            var streamPipeSource = new StreamPipeSource(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);
            using var instance = PrepareInstance(pipeArgument.PipePath, outputCapacity, ffOptions ?? GlobalFFOptions.Current);
            pipeArgument.Pre();

            var task = instance.FinishedRunning();
            try
            {
                pipeArgument.During().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (IOException) { }
            finally
            {
                pipeArgument.Post();
            }
            var exitCode = task.ConfigureAwait(false).GetAwaiter().GetResult();
            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({exitCode} - {string.Join("\n", instance.ErrorData)})", null, string.Join("\n", instance.ErrorData));
            
            return ParseOutput(instance);
        }
        public static async Task<IMediaAnalysis> AnalyseAsync(string filePath, int outputCapacity = int.MaxValue, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath)) 
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");
            
            using var instance = PrepareInstance(filePath, outputCapacity, ffOptions ?? GlobalFFOptions.Current);
            await instance.FinishedRunning().ConfigureAwait(false);
            return ParseOutput(instance);
        }
        public static async Task<IMediaAnalysis> AnalyseAsync(Uri uri, int outputCapacity = int.MaxValue, FFOptions? ffOptions = null)
        {
            using var instance = PrepareInstance(uri.AbsoluteUri, outputCapacity, ffOptions ?? GlobalFFOptions.Current);
            await instance.FinishedRunning().ConfigureAwait(false);
            return ParseOutput(instance);
        }
        public static async Task<IMediaAnalysis> AnalyseAsync(Stream stream, int outputCapacity = int.MaxValue, FFOptions? ffOptions = null)
        {
            var streamPipeSource = new StreamPipeSource(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);
            using var instance = PrepareInstance(pipeArgument.PipePath, outputCapacity, ffOptions ?? GlobalFFOptions.Current);
            pipeArgument.Pre();

            var task = instance.FinishedRunning();
            try
            {
                await pipeArgument.During().ConfigureAwait(false);
            }
            catch(IOException)
            {
            }
            finally
            {
                pipeArgument.Post();
            }
            var exitCode = await task.ConfigureAwait(false);
            if (exitCode != 0)
                throw new FFProbeProcessException($"ffprobe exited with non-zero exit-code ({exitCode} - {string.Join("\n", instance.ErrorData)})", instance.ErrorData);
            
            pipeArgument.Post();
            return ParseOutput(instance);
        }

        private static IMediaAnalysis ParseOutput(Instance instance)
        {
            var json = string.Join(string.Empty, instance.OutputData);
            var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbeAnalysis>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (ffprobeAnalysis?.Format == null)
                throw new FormatNullException();
            
            return new MediaAnalysis(ffprobeAnalysis);
        }

        private static Instance PrepareInstance(string filePath, int outputCapacity, FFOptions ffOptions)
        {
            FFProbeHelper.RootExceptionCheck();
            FFProbeHelper.VerifyFFProbeExists(ffOptions);
            var arguments = $"-loglevel error -print_format json -show_format -sexagesimal -show_streams \"{filePath}\"";
            var startInfo = new ProcessStartInfo(GlobalFFOptions.GetFFProbeBinaryPath(), arguments)
            {
                StandardOutputEncoding = ffOptions.Encoding,
                StandardErrorEncoding = ffOptions.Encoding
            };
            var instance = new Instance(startInfo) { DataBufferCapacity = outputCapacity };
            return instance;
        }
    }
}
