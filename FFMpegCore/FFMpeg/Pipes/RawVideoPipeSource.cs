using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeSource"/> for a raw video stream that is gathered from <see cref="IEnumerator{IVideoFrame}"/> 
    /// </summary>
    public class RawVideoPipeSource : IPipeSource
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public string Format { get; private set; }
        public int FrameRate { get; set; } = 25;
        private bool _formatInitialized;
        private readonly IEnumerator<IVideoFrame> _framesEnumerator;

        public RawVideoPipeSource(IEnumerator<IVideoFrame> framesEnumerator)
        {
            _framesEnumerator = framesEnumerator;
        }

        public RawVideoPipeSource(IEnumerable<IVideoFrame> framesEnumerator) : this(framesEnumerator.GetEnumerator()) { }

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
                Format = _framesEnumerator.Current!.Format;
                Width = _framesEnumerator.Current!.Width;
                Height = _framesEnumerator.Current!.Height;

                _formatInitialized = true;
            }

            return $"-f rawvideo -r {FrameRate} -pix_fmt {Format} -s {Width}x{Height}";
        }

        public async Task WriteAsync(System.IO.Stream outputStream, CancellationToken cancellationToken)
        {
            if (_framesEnumerator.Current != null)
            {
                CheckFrameAndThrow(_framesEnumerator.Current);
                await _framesEnumerator.Current.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }

            while (_framesEnumerator.MoveNext())
            {
                CheckFrameAndThrow(_framesEnumerator.Current!);
                await _framesEnumerator.Current!.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }
        }

        private void CheckFrameAndThrow(IVideoFrame frame)
        {
            if (frame.Width != Width || frame.Height != Height || frame.Format != Format)
                throw new FFMpegException(FFMpegExceptionType.Operation, "Video frame is not the same format as created raw video stream\r\n" +
                    $"Frame format: {frame.Width}x{frame.Height} pix_fmt: {frame.Format}\r\n" +
                    $"Stream format: {Width}x{Height} pix_fmt: {Format}");
        }
    }
}
