using System.Globalization;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Pipes;

/// <summary>
///     Implementation of <see cref="IPipeSource" /> for a raw video stream that is gathered from <see cref="IEnumerator{IVideoFrame}" />
/// </summary>
public class RawVideoPipeSource : IPipeSource
{
    private readonly IEnumerator<IVideoFrame> _framesEnumerator;
    private IVideoFrame? _firstFrame;

    public RawVideoPipeSource(IEnumerable<IVideoFrame> framesEnumerator)
    {
        _framesEnumerator = framesEnumerator.GetEnumerator();
    }

    public string PixelFormat { get; private set; } = null!;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public double FrameRate { get; set; } = 25;

    public string GetStreamArguments()
    {
        var firstFrame = FirstFrame();
        return $"-f rawvideo -r {FrameRate.ToString(CultureInfo.InvariantCulture)} -pix_fmt {firstFrame.PixelFormat} -s {firstFrame.Width}x{firstFrame.Height}";
    }

    public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        await FirstFrame().SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
        while (_framesEnumerator.MoveNext())
        {
            CheckFrameAndThrow(_framesEnumerator.Current!);
            await _framesEnumerator.Current!.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
        }
    }

    private IVideoFrame FirstFrame()
    {
        if (_firstFrame == null)
        {
            if (!_framesEnumerator.MoveNext() || _framesEnumerator.Current == null)
            {
                throw new InvalidOperationException("Enumerator is empty, unable to get frame");
            }

            _firstFrame = _framesEnumerator.Current;
            PixelFormat = _firstFrame.PixelFormat;
            Width = _firstFrame.Width;
            Height = _firstFrame.Height;
        }

        return _firstFrame;
    }

    private void CheckFrameAndThrow(IVideoFrame frame)
    {
        if (frame.Width != Width || frame.Height != Height || frame.PixelFormat != PixelFormat)
        {
            throw new FFMpegStreamFormatException(FFMpegExceptionType.Operation, "Video frame is not the same format as created raw video stream\r\n" +
                                                                                 $"Frame format: {frame.Width}x{frame.Height} pix_fmt: {frame.PixelFormat}\r\n" +
                                                                                 $"Stream format: {Width}x{Height} pix_fmt: {PixelFormat}");
        }
    }
}
