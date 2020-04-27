using FFMpegCore.FFMPEG.Argument;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeSource"/> for a raw video stream that is gathered from <see cref="IEnumerator{IVideoFrame}"/> 
    /// </summary>
    public class RawVideoPipeSource : IPipeSource
    {
        public string StreamFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FrameRate { get; set; } = 25;
        private IEnumerator<IVideoFrame> framesEnumerator;

        public RawVideoPipeSource(IEnumerator<IVideoFrame> framesEnumerator)
        {
            this.framesEnumerator = framesEnumerator;
        }

        public RawVideoPipeSource(IEnumerable<IVideoFrame> framesEnumerator) : this(framesEnumerator.GetEnumerator()) { }

        public string GetFormat()
        {
            //see input format references https://lists.ffmpeg.org/pipermail/ffmpeg-user/2012-July/007742.html
            if (framesEnumerator.Current == null)
            {
                if (!framesEnumerator.MoveNext())
                    throw new InvalidOperationException("Enumerator is empty, unable to get frame");

                StreamFormat = framesEnumerator.Current.Format;
                Width = framesEnumerator.Current.Width;
                Height = framesEnumerator.Current.Height;
            }

            return $"-f rawvideo -r {FrameRate} -pix_fmt {StreamFormat} -s {Width}x{Height}";
        }

        public void FlushData(System.IO.Stream stream)
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

        public async Task FlushDataAsync(System.IO.Stream stream)
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
