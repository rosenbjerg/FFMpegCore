using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Builds parameters string from <see cref="ArgumentsContainer"/> that would be passed to ffmpeg process
    /// </summary>
    public class FFArgumentBuilder : IArgumentBuilder
    {
        /// <summary>
        /// Builds parameters string from <see cref="ArgumentsContainer"/> that would be passed to ffmpeg process
        /// </summary>
        /// <param name="container">Container of arguments</param>
        /// <returns>Parameters string</returns>
        public string BuildArguments(ArgumentsContainer container)
        {
            if (!container.ContainsInputOutput())
                throw new ArgumentException("No input or output parameter found", nameof(container));

            CheckContainerException(container);

            StringBuilder sb = new StringBuilder();

            sb.Append(GetInput(container).GetStringValue().Trim() + " ");

            foreach(var a in container)
            {
                if(!IsInputOrOutput(a.Key))
                {
                    sb.Append(a.Value.GetStringValue().Trim() + " ");
                }
            }

            sb.Append(container[typeof(OutputArgument)].GetStringValue().Trim());

            var overrideArg = container.Find<OverrideArgument>();
            if (overrideArg != null)
                sb.Append(" " + overrideArg.GetStringValue().Trim());

            return sb.ToString();
        }

        /// <summary>
        /// Builds parameters string from <see cref="ArgumentsContainer"/> that would be passed to ffmpeg process
        /// </summary>
        /// <param name="container">Container of arguments</param>
        /// <param name="input">Input file</param>
        /// <param name="output">Output file</param>
        /// <returns>Parameters string</returns>
        public string BuildArguments(ArgumentsContainer container, FileInfo input, FileInfo output)
        {
            CheckContainerException(container);
            CheckExtensionOfOutputExtension(container, output);
            FFMpegHelper.ConversionExceptionCheck(input, output);


            StringBuilder sb = new StringBuilder();

            var inputA = new InputArgument(input);
            var outputA = new OutputArgument();

            sb.Append(inputA.GetStringValue().Trim() + " ");

            foreach (var a in container)
            {
                if (!IsInputOrOutput(a.Key))
                {
                    sb.Append(a.Value.GetStringValue().Trim() + " ");
                }
            }

            sb.Append(outputA.GetStringValue().Trim());

            var overrideArg = container.Find<OverrideArgument>();
            if (overrideArg != null)
                sb.Append(" " + overrideArg.GetStringValue().Trim());

            return sb.ToString();
        }

        private void CheckContainerException(ArgumentsContainer container)
        {
            //TODO: implement arguments check            
        }

        private void CheckExtensionOfOutputExtension(ArgumentsContainer container, FileInfo output)
        {
            if(container.ContainsKey(typeof(VideoCodecArgument)))
            {
                var codec = (VideoCodecArgument)container[typeof(VideoCodecArgument)];
                FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.ForCodec(codec.Value));
            }
        }

        private Argument GetInput(ArgumentsContainer container)
        {
            if (container.ContainsKey(typeof(InputArgument)))
                return container[typeof(InputArgument)];
            else if (container.ContainsKey(typeof(ConcatArgument)))
                return container[typeof(ConcatArgument)];
            else
                throw new ArgumentException("No inputs found");
        }

        private bool IsInputOrOutput(Argument arg)
        {
            return IsInputOrOutput(arg.GetType());
        }

        private bool IsInputOrOutput(Type arg)
        {
            return (arg == typeof(InputArgument)) || (arg == typeof(ConcatArgument)) || (arg == typeof(OutputArgument)) || (arg == typeof(OverrideArgument));
        }
    }
}
