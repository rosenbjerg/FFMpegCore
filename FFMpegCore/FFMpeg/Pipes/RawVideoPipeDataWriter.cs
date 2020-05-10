using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeDataWriter"/> for a raw video stream that is gathered from <see cref="IEnumerator{IVideoFrame}"/> 
    /// </summary>
    public class RawVideoPipeDataWriter : IPipeDataWriter
    {
        public string StreamFormat { get; private set; } = null!;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FrameRate { get; set; } = 25;
        private bool _formatInitialized;
        private readonly IEnumerator<IVideoFrame> _framesEnumerator;

        public RawVideoPipeDataWriter(IEnumerator<IVideoFrame> framesEnumerator)
        {
            _framesEnumerator = framesEnumerator;
        }

        public RawVideoPipeDataWriter(IEnumerable<IVideoFrame> framesEnumerator) : this(framesEnumerator.GetEnumerator()) { }

        public string GetFormat()
        {
            if (!_formatInitialized)
            {
                //see input format references https://lists.ffmpeg.org/pipermail/ffmpeg-user/2012-July/007742.html
                if (_framesEnumerator.Current == null)
                {
                    if (!_framesEnumerator.MoveNext())
                        throw new InvalidOperationException("Enumerator is empty, unable to get frame");
                }
                StreamFormat = _framesEnumerator.Current!.Format;
                Width = _framesEnumerator.Current!.Width;
                Height = _framesEnumerator.Current!.Height;

                _formatInitialized = true;
            }

            return $"-f rawvideo -r {FrameRate} -pix_fmt {StreamFormat} -s {Width}x{Height}";
        }

        public void WriteData(System.IO.Stream stream)
        {
            if (_framesEnumerator.Current != null)
            {
                CheckFrameAndThrow(_framesEnumerator.Current);
                _framesEnumerator.Current.Serialize(stream);
            }

            while (_framesEnumerator.MoveNext())
            {
                CheckFrameAndThrow(_framesEnumerator.Current!);
                _framesEnumerator.Current!.Serialize(stream);
            }
        }

        public async Task WriteDataAsync(System.IO.Stream stream, CancellationToken token)
        {
            if (_framesEnumerator.Current != null)
            {
                await _framesEnumerator.Current.SerializeAsync(stream, token).ConfigureAwait(false);
            }

            while (_framesEnumerator.MoveNext())
            {
                await _framesEnumerator.Current!.SerializeAsync(stream, token).ConfigureAwait(false);
            }
        }

        private void CheckFrameAndThrow(IVideoFrame frame)
        {
            if (frame.Width != Width || frame.Height != Height || frame.Format != StreamFormat)
                throw new FFMpegException(FFMpegExceptionType.Operation, "Video frame is not the same format as created raw video stream\r\n" +
                    $"Frame format: {frame.Width}x{frame.Height} pix_fmt: {frame.Format}\r\n" +
                    $"Stream format: {Width}x{Height} pix_fmt: {StreamFormat}");
        }
    }
}
