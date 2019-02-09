using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using System;
using System.IO;
using System.Linq;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Builds parameters string from <see cref="ArgumentContainer"/> that would be passed to ffmpeg process
    /// </summary>
    public class FFArgumentBuilder : IArgumentBuilder
    {
        /// <summary>
        /// Builds parameters string from <see cref="ArgumentContainer"/> that would be passed to ffmpeg process
        /// </summary>
        /// <param name="container">Container of arguments</param>
        /// <returns>Parameters string</returns>
        public string BuildArguments(ArgumentContainer container)
        {
            if (!container.ContainsInputOutput())
                throw new ArgumentException("No input or output parameter found", nameof(container));
            

            return string.Join(" ", container.Select(argument => argument.Value.GetStringValue().Trim()));
        }

        private void CheckExtensionOfOutputExtension(ArgumentContainer container, FileInfo output)
        {
            if(container.ContainsKey(typeof(VideoCodecArgument)))
            {
                var codec = (VideoCodecArgument)container[typeof(VideoCodecArgument)];
                FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.ForCodec(codec.Value));
            }
        }

        private Argument GetInput(ArgumentContainer container)
        {
            if (container.ContainsKey(typeof(InputArgument)))
                return container[typeof(InputArgument)];
            else if (container.ContainsKey(typeof(ConcatArgument)))
                return container[typeof(ConcatArgument)];
            else
                throw new ArgumentException("No inputs found");
        }
    }
}
