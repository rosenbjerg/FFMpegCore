using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Extensions.System.Drawing.Common;

public static class SystemDrawingImageExtensions
{
    public static FFMpegArgumentProcessor AddAudio(this Image poster, string audio, string output, Codec? audioCodec = null,
        Action<FFMpegOutputOptions>? addArguments = null, FFOptions? ffOptions = null)
    {
        return poster.AddAudio(FFProbe.Analyse(audio, ffOptions), output, audioCodec, addArguments, ffOptions);
    }

    public static FFMpegArgumentProcessor AddAudio(this Image poster, IMediaAnalysis audioSource, string output, Codec? audioCodec = null,
        Action<FFMpegOutputOptions>? addArguments = null, FFOptions? ffOptions = null)
    {
        return FFMpeg.PosterWithAudio(new SystemDrawingPosterInputArgument(poster), new Size(poster.Width, poster.Height), audioSource, output, audioCodec,
            addArguments, ffOptions);
    }
}
