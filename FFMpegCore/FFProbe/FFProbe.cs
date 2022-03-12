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
        public static IMediaAnalysis Analyse(string filePath, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath)) 
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");
            
            var processArguments = PrepareStreamAnalysisInstance(filePath, ffOptions ?? GlobalFFOptions.Current);
            var result = processArguments.StartAndWaitForExit();
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));
            
            return ParseOutput(result);
        }
        public static FFProbeFrames GetFrames(string filePath, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath))
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");

            var instance = PrepareFrameAnalysisInstance(filePath, ffOptions ?? GlobalFFOptions.Current);
            var result = instance.StartAndWaitForExit();
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));

            return ParseFramesOutput(result);
        }

        public static FFProbePackets GetPackets(string filePath, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath))
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");

            var instance = PreparePacketAnalysisInstance(filePath, ffOptions ?? GlobalFFOptions.Current);
            var result = instance.StartAndWaitForExit();
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));

            return ParsePacketsOutput(result);
        }

        public static IMediaAnalysis Analyse(Uri uri, FFOptions? ffOptions = null)
        {
            var instance = PrepareStreamAnalysisInstance(uri.AbsoluteUri, ffOptions ?? GlobalFFOptions.Current);
            var result = instance.StartAndWaitForExit();
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));

            return ParseOutput(result);
        }
        public static IMediaAnalysis Analyse(Stream stream, FFOptions? ffOptions = null)
        {
            var streamPipeSource = new StreamPipeSource(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);
            var instance = PrepareStreamAnalysisInstance(pipeArgument.PipePath, ffOptions ?? GlobalFFOptions.Current);
            pipeArgument.Pre();

            var task = instance.StartAndWaitForExitAsync();
            try
            {
                pipeArgument.During().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (IOException) { }
            finally
            {
                pipeArgument.Post();
            }
            var result = task.ConfigureAwait(false).GetAwaiter().GetResult();
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));
            
            return ParseOutput(result);
        }
        public static async Task<IMediaAnalysis> AnalyseAsync(string filePath, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath)) 
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");
            
            var instance = PrepareStreamAnalysisInstance(filePath, ffOptions ?? GlobalFFOptions.Current);
            var result = await instance.StartAndWaitForExitAsync().ConfigureAwait(false);
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));

            return ParseOutput(result);
        }

        public static async Task<FFProbeFrames> GetFramesAsync(string filePath, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath))
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");

            var instance = PrepareFrameAnalysisInstance(filePath, ffOptions ?? GlobalFFOptions.Current);
            var result = await instance.StartAndWaitForExitAsync().ConfigureAwait(false);
            return ParseFramesOutput(result);
        }

        public static async Task<FFProbePackets> GetPacketsAsync(string filePath, FFOptions? ffOptions = null)
        {
            if (!File.Exists(filePath))
                throw new FFMpegException(FFMpegExceptionType.File, $"No file found at '{filePath}'");

            var instance = PreparePacketAnalysisInstance(filePath, ffOptions ?? GlobalFFOptions.Current);
            var result = await instance.StartAndWaitForExitAsync().ConfigureAwait(false);
            return ParsePacketsOutput(result);
        }

        public static async Task<IMediaAnalysis> AnalyseAsync(Uri uri, FFOptions? ffOptions = null)
        {
            var instance = PrepareStreamAnalysisInstance(uri.AbsoluteUri, ffOptions ?? GlobalFFOptions.Current);
            var result = await instance.StartAndWaitForExitAsync().ConfigureAwait(false);
            if (result.ExitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", null, string.Join("\n", result.ErrorData));

            return ParseOutput(result);
        }
        public static async Task<IMediaAnalysis> AnalyseAsync(Stream stream, FFOptions? ffOptions = null)
        {
            var streamPipeSource = new StreamPipeSource(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);
            var instance = PrepareStreamAnalysisInstance(pipeArgument.PipePath, ffOptions ?? GlobalFFOptions.Current);
            pipeArgument.Pre();

            var task = instance.StartAndWaitForExitAsync();
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
            var result = await task.ConfigureAwait(false);
            if (result.ExitCode != 0)
                throw new FFProbeProcessException($"ffprobe exited with non-zero exit-code ({result.ExitCode} - {string.Join("\n", result.ErrorData)})", result.ErrorData);
            
            pipeArgument.Post();
            return ParseOutput(result);
        }

        private static IMediaAnalysis ParseOutput(IProcessResult instance)
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
        private static FFProbeFrames ParseFramesOutput(IProcessResult instance)
        {
            var json = string.Join(string.Empty, instance.OutputData);
            var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbeFrames>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.WriteAsString
            }) ;

            return ffprobeAnalysis;
        }

        private static FFProbePackets ParsePacketsOutput(IProcessResult instance)
        {
            var json = string.Join(string.Empty, instance.OutputData);
            var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbePackets>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.WriteAsString
            }) ;

            return ffprobeAnalysis;
        }


        private static ProcessArguments PrepareStreamAnalysisInstance(string filePath, FFOptions ffOptions)
            => PrepareInstance($"-loglevel error -print_format json -show_format -sexagesimal -show_streams \"{filePath}\"", ffOptions);
        private static ProcessArguments PrepareFrameAnalysisInstance(string filePath, FFOptions ffOptions)
            => PrepareInstance($"-loglevel error -print_format json -show_frames -v quiet -sexagesimal \"{filePath}\"", ffOptions);
        private static ProcessArguments PreparePacketAnalysisInstance(string filePath, FFOptions ffOptions)
            => PrepareInstance($"-loglevel error -print_format json -show_packets -v quiet -sexagesimal \"{filePath}\"", ffOptions);
        
        private static ProcessArguments PrepareInstance(string arguments, FFOptions ffOptions)
        {
            FFProbeHelper.RootExceptionCheck();
            FFProbeHelper.VerifyFFProbeExists(ffOptions);
            var startInfo = new ProcessStartInfo(GlobalFFOptions.GetFFProbeBinaryPath(), arguments)
            {
                StandardOutputEncoding = ffOptions.Encoding,
                StandardErrorEncoding = ffOptions.Encoding,
                WorkingDirectory = ffOptions.WorkingDirectory
            };
            return new ProcessArguments(startInfo);
        }
    }
}
