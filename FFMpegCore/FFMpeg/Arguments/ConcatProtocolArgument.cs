namespace FFMpegCore.Arguments;

/// <summary>
///     ffmpeg's <c>concat:</c> protocol, which joins the files byte-wise before demuxing. Only formats that survive naive
///     concatenation, such as mpegts and mp3, work this way; for everything else use <see cref="ConcatDemuxerArgument" />.
/// </summary>
public class ConcatProtocolArgument : IInputArgument
{
    public readonly IEnumerable<string> Values;

    public ConcatProtocolArgument(IEnumerable<string> values)
    {
        Values = values;
    }

    public void Pre(FFOptions options) { }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post() { }

    public string Text => $"-i \"concat:{string.Join(@"|", Values)}\"";
}
