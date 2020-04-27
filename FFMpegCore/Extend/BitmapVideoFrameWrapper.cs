using FFMpegCore.FFMPEG.Pipes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.Extend
{
    public class BitmapVideoFrameWrapper : IVideoFrame, IDisposable
    {
        public int Width => Source.Width;

        public int Height => Source.Height;

        public string Format { get; private set; }

        public Bitmap Source { get; private set; }

        public BitmapVideoFrameWrapper(Bitmap bitmap)
        {
            Source = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Format = ConvertStreamFormat(bitmap.PixelFormat);
        }

        public void Serialize(IInputPipe pipe)
        {
            var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);

            try
            {
                var buffer = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                pipe.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                Source.UnlockBits(data);
            }
        }

        public async Task SerializeAsync(IInputPipe pipe)
        {
            var data = Source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, Source.PixelFormat);

            try
            {
                var buffer = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                await pipe.WriteAsync(buffer, 0, buffer.Length);
            }
            finally
            {
                Source.UnlockBits(data);
            }
        }

        public void Dispose()
        {
            Source.Dispose();
        }

        private static string ConvertStreamFormat(PixelFormat fmt)
        {
            switch (fmt)
            {
                case PixelFormat.Format16bppGrayScale:
                    return "gray16le";
                case PixelFormat.Format16bppRgb565:
                    return "bgr565le";
                case PixelFormat.Format24bppRgb:
                    return "rgb24";
                case PixelFormat.Format32bppArgb:
                    return "rgba";
                case PixelFormat.Format32bppPArgb:
                    //This is not really same as argb32
                    return "argb";
                case PixelFormat.Format32bppRgb:
                    return "rgba";
                case PixelFormat.Format48bppRgb:
                    return "rgb48le";
                default:
                    throw new NotSupportedException($"Not supported pixel format {fmt}");
            }
        }
    }
}
