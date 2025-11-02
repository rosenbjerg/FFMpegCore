using FFMpegCore.Arguments.Formats;

namespace FFMpegCore.Arguments.Protocols.Rtsp;

public interface IRtspMuxerArgument :
    IRtspArgument,
    IDemuxerArgument;
