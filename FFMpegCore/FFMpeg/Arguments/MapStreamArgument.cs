using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents choice of stream by the stream specifier
/// </summary>
public class MapStreamArgument : IArgument
{
    private readonly int _inputFileIndex;
    private readonly bool _negativeMap;
    private readonly int _streamIndex;
    private readonly StreamType _streamType;

    public MapStreamArgument(int streamIndex, int inputFileIndex, StreamType streamType = StreamType.All, bool negativeMap = false)
    {
        _inputFileIndex = inputFileIndex;
        _streamIndex = streamIndex;
        _streamType = streamType;
        _negativeMap = negativeMap;
    }

    public string Text => $"-map {(_negativeMap ? "-" : "")}{_inputFileIndex}{_streamType.Specifier()}:{_streamIndex}";
}
