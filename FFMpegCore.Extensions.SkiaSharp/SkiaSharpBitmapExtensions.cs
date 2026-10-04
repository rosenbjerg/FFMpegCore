using FFMpegCore.Enums;
using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp;

public static class SkiaSharpBitmapExtensions
{
    public static FFMpegArgumentProcessor AddAudio(this SKBitmap poster, string audio, string output, Codec? audioCodec = null, FFOptions? ffOptions = null)
    {
        return poster.AddAudio(FFProbe.Analyse(audio, ffOptions), output, audioCodec, ffOptions);
    }

    public static FFMpegArgumentProcessor AddAudio(this SKBitmap poster, IMediaAnalysis audioSource, string output, Codec? audioCodec = null,
        FFOptions? ffOptions = null)
    {
        return FFMpeg.PosterWithAudio(new SkiaSharpPosterInputArgument(poster), new System.Drawing.Size(poster.Width, poster.Height), audioSource, output, audioCodec,
            ffOptions);
    }
}
