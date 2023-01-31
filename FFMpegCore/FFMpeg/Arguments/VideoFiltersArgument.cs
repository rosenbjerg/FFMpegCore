using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments
{
    public class VideoFiltersArgument : IArgument
    {
        public readonly VideoFilterOptions Options;
        
        public VideoFiltersArgument(VideoFilterOptions options)
        {
            Options = options;
        }

        public string Text => GetText();

        private string GetText()
        {
            if (!Options.Arguments.Any())
                throw new FFMpegArgumentException("No video-filter arguments provided");

            var arguments = Options.Arguments
                .Where(arg => !string.IsNullOrEmpty(arg.Value))
                .Select(arg =>
                {
                    var escapedValue = arg.Value.Replace(",", "\\,");
                    return string.IsNullOrEmpty(arg.Key) ? escapedValue : $"{arg.Key}={escapedValue}";
                });

            return $"-vf \"{string.Join(", ", arguments)}\"";
        }
    }

    public interface IVideoFilterArgument
    {
        public string Key { get; }
        public string Value { get; }
    }

    public class VideoFilterOptions
    {
        public List<IVideoFilterArgument> Arguments { get; } = new List<IVideoFilterArgument>();
        
        public VideoFilterOptions Scale(VideoSize videoSize) => WithArgument(new ScaleArgument(videoSize));
        public VideoFilterOptions Scale(int width, int height) => WithArgument(new ScaleArgument(width, height));
        public VideoFilterOptions Scale(Size size) => WithArgument(new ScaleArgument(size));
        public VideoFilterOptions Transpose(Transposition transposition) => WithArgument(new TransposeArgument(transposition));
        public VideoFilterOptions Mirror(Mirroring mirroring) => WithArgument(new SetMirroringArgument(mirroring));
        public VideoFilterOptions DrawText(DrawTextOptions drawTextOptions) => WithArgument(new DrawTextArgument(drawTextOptions));
        public VideoFilterOptions HardBurnSubtitle(SubtitleHardBurnOptions subtitleHardBurnOptions) => WithArgument(new SubtitleHardBurnArgument(subtitleHardBurnOptions));
        public VideoFilterOptions BlackDetect(double minimumDuration = 2.0, double pictureBlackRatioThreshold = 0.98, double pixelBlackThreshold = 0.1) => WithArgument(new BlackDetectArgument(minimumDuration, pictureBlackRatioThreshold, pixelBlackThreshold));
        public VideoFilterOptions BlackFrame(int amount = 98, int threshold = 32) => WithArgument(new BlackFrameArgument(amount, threshold));
        public VideoFilterOptions Pad(PadOptions padOptions) => WithArgument(new PadArgument(padOptions));

        private VideoFilterOptions WithArgument(IVideoFilterArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}