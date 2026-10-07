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
        var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);

        try
        {
            var buffer = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
        }
        finally
        {
            Source.UnlockBits(data);
        }
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token)
    {
        var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);

        try
        {
            var buffer = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            await stream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
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
                return "bgr555le";
            case DrawingPixelFormat.Format16bppRgb565:
                return "bgr565le";
            case DrawingPixelFormat.Format24bppRgb:
                return "bgr24";
            case DrawingPixelFormat.Format32bppArgb:
                return "bgra";
            case DrawingPixelFormat.Format32bppPArgb:
                //This is not really same as argb32
                return "argb";
            case DrawingPixelFormat.Format32bppRgb:
                return "rgba";
            case DrawingPixelFormat.Format48bppRgb:
                return "rgb48le";
            default:
                throw new NotSupportedException($"Not supported pixel format {fmt}");
        }
    }
}
