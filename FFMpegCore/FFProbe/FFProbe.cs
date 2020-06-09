using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static MediaAnalysis Analyse(string filePath, int outputCapacity = int.MaxValue)
        {
            using var instance = PrepareInstance(filePath, outputCapacity);
            instance.BlockUntilFinished();
            return ParseOutput(filePath, instance);
        }
        public static MediaAnalysis Analyse(System.IO.Stream stream, int outputCapacity = int.MaxValue)
        {
            var streamPipeSource = new StreamPipeSource(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);
            using var instance = PrepareInstance(pipeArgument.PipePath, outputCapacity);
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
                throw new FFMpegException(FFMpegExceptionType.Process, $"FFProbe process returned exit status {exitCode}: {string.Join("\n", instance.OutputData)} {string.Join("\n", instance.ErrorData)}");
            
            return ParseOutput(pipeArgument.PipePath, instance);
        }
        public static async Task<MediaAnalysis> AnalyseAsync(string filePath, int outputCapacity = int.MaxValue)
        {
            using var instance = PrepareInstance(filePath, outputCapacity);
            await instance.FinishedRunning();
            return ParseOutput(filePath, instance);
        }
        public static async Task<MediaAnalysis> AnalyseAsync(System.IO.Stream stream, int outputCapacity = int.MaxValue)
        {
            var streamPipeSource = new StreamPipeSource(stream);
            var pipeArgument = new InputPipeArgument(streamPipeSource);
            using var instance = PrepareInstance(pipeArgument.PipePath, outputCapacity);
            pipeArgument.Pre();

            var task = instance.FinishedRunning();
            try
            {
                await pipeArgument.During();
            }
            catch(IOException)
            {
            }
            finally
            {
                pipeArgument.Post();
            }
            var exitCode = await task;
            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, $"FFProbe process returned exit status {exitCode}: {string.Join("\n", instance.OutputData)} {string.Join("\n", instance.ErrorData)}");
            
            pipeArgument.Post();
            return ParseOutput(pipeArgument.PipePath, instance);
        }

        private static MediaAnalysis ParseOutput(string filePath, Instance instance)
        {
            var json = string.Join(string.Empty, instance.OutputData);
            var ffprobeAnalysis = JsonSerializer.Deserialize<FFProbeAnalysis>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return new MediaAnalysis(filePath, ffprobeAnalysis, GetMainDuration(instance.ErrorData));
        }

        private static Instance PrepareInstance(string filePath, int outputCapacity)
        {
            FFProbeHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);
            var ffprobe = FFMpegOptions.Options.FFProbeBinary();
            var arguments = $"-print_format json -show_streams \"{filePath}\"";
            var instance = new Instance(ffprobe, arguments) {DataBufferCapacity = outputCapacity};
            return instance;
        }

        private static TimeSpan? GetMainDuration(IReadOnlyList<string> mainOutput)
        {
	        const string durationPattern = "Duration:";
	        var durationLine = mainOutput.FirstOrDefault(x => x.Contains(durationPattern));
			if (string.IsNullOrWhiteSpace(durationLine))
			{
				return null;
			}

			var durationPos = durationLine.IndexOf(durationPattern, StringComparison.InvariantCulture);
            if(durationPos < 0)
            {
	            return null;
            }

            durationLine = durationLine.Substring(durationPos + durationPattern.Length);

            var commaPos = durationLine.IndexOf(',');
            if (commaPos < 0)
            {
	            return null;
            }
            durationLine = durationLine.Substring(0, commaPos);

            if(TimeSpan.TryParse(durationLine, out var mainDuration))
            {
	            return mainDuration;
            }

            return null;
        }
    }
}
