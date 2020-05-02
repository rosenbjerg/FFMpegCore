using FFMpegCore.FFMPEG.Argument;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeDataWriter"/> for a raw video stream that is gathered from <see cref="IEnumerator{IVideoFrame}"/> 
    /// </summary>
    public class RawVideoPipeDataWriter : IPipeDataWriter
    {
        public string StreamFormat { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FrameRate { get; set; } = 25;
        private IEnumerator<IVideoFrame> framesEnumerator;

        public RawVideoPipeDataWriter(IEnumerator<IVideoFrame> framesEnumerator)
        {
            this.framesEnumerator = framesEnumerator;
        }

        public RawVideoPipeDataWriter(IEnumerable<IVideoFrame> framesEnumerator) : this(framesEnumerator.GetEnumerator()) { }

        public string GetFormat()
        {
            //see input format references https://lists.ffmpeg.org/pipermail/ffmpeg-user/2012-July/007742.html
            if (framesEnumerator.Current == null)
            {
                if (!framesEnumerator.MoveNext())
                    throw new InvalidOperationException("Enumerator is empty, unable to get frame");
            }
            StreamFormat = framesEnumerator.Current.Format;
            Width = framesEnumerator.Current.Width;
            Height = framesEnumerator.Current.Height;


            return $"-f rawvideo -r {FrameRate} -pix_fmt {StreamFormat} -s {Width}x{Height}";
        }

        public void WriteData(System.IO.Stream stream)
        {
            if (framesEnumerator.Current != null)
            {
                framesEnumerator.Current.Serialize(stream);
            }

            while (framesEnumerator.MoveNext())
            {
                framesEnumerator.Current.Serialize(stream);
            }
        }

        public async Task WriteDataAsync(System.IO.Stream stream)
        {
            if (framesEnumerator.Current != null)
            {
                await framesEnumerator.Current.SerializeAsync(stream);
            }

            while (framesEnumerator.MoveNext())
            {
                await framesEnumerator.Current.SerializeAsync(stream);
            }
        }

    }
}
