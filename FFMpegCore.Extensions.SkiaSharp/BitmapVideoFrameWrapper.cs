using FFMpegCore.Pipes;
using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp
{
    public class BitmapVideoFrameWrapper : IVideoFrame, IDisposable
    {
        public int Width => Source.Width;

        public int Height => Source.Height;

        public string Format { get; private set; }

        public SKBitmap Source { get; private set; }

        public BitmapVideoFrameWrapper(SKBitmap bitmap)
        {
            Source = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Format = ConvertStreamFormat(bitmap.ColorType);
        }

        public void Serialize(Stream stream)
        {
            var data = Source.Bytes;
            stream.Write(data, 0, data.Length);
        }

        public async Task SerializeAsync(Stream stream, CancellationToken token)
        {
            var data = Source.Bytes;
            await stream.WriteAsync(data, 0, data.Length, token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Source.Dispose();
        }

        private static string ConvertStreamFormat(SKColorType fmt)
        {
            switch (fmt)
            {
                case SKColorType.Gray8:
                    return "gray8";
                case SKColorType.Bgra8888:
                    return "bgra";
                case SKColorType.Rgb888x:
                    return "rgb";
                case SKColorType.Rgba8888:
                    return "rgba";
                case SKColorType.Rgb565:
                    return "rgb565";
                default:
                    throw new NotSupportedException($"Not supported pixel format {fmt}");
            }
        }
    }
}
