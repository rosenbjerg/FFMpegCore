using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FFMpegCore.Pipes;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace FFMpegCore.Extensions.System.Drawing.Common;

public class SystemDrawingVideoFrame : IVideoFrame, IDisposable
{
    public SystemDrawingVideoFrame(Bitmap bitmap)
    {
        Source = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        PixelFormat = ConvertStreamFormat(bitmap.PixelFormat);
    }

    public Bitmap Source { get; }

    public void Dispose()
    {
        Source.Dispose();
    }

    public int Width => Source.Width;

    public int Height => Source.Height;

    public string PixelFormat { get; }

    public void Serialize(Stream stream)
    {
        var pixels = ReadPixels();
        stream.Write(pixels, 0, pixels.Length);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token)
    {
        var pixels = ReadPixels();
        await stream.WriteAsync(pixels, 0, pixels.Length, token).ConfigureAwait(false);
    }

    private byte[] ReadPixels()
    {
        var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);
        try
        {
            var rowLength = Width * Image.GetPixelFormatSize(Source.PixelFormat) / 8;
            var pixels = new byte[rowLength * Height];
            for (var row = 0; row < Height; row++)
            {
                Marshal.Copy(data.Scan0 + row * data.Stride, pixels, row * rowLength, rowLength);
            }

            return pixels;
        }
        finally
        {
            Source.UnlockBits(data);
        }
    }

    private static string ConvertStreamFormat(DrawingPixelFormat fmt)
    {
        switch (fmt)
        {
            case DrawingPixelFormat.Format16bppGrayScale:
                return "gray16le";
            case DrawingPixelFormat.Format16bppRgb555:
                return "rgb555le";
            case DrawingPixelFormat.Format16bppRgb565:
                return "rgb565le";
            case DrawingPixelFormat.Format24bppRgb:
                return "bgr24";
            case DrawingPixelFormat.Format32bppArgb:
                return "bgra";
            case DrawingPixelFormat.Format32bppPArgb:
                return "bgra";
            case DrawingPixelFormat.Format32bppRgb:
                return "bgr0";
            case DrawingPixelFormat.Format48bppRgb:
                return "rgb48le";
            default:
                throw new NotSupportedException($"Not supported pixel format {fmt}");
        }
    }
}
