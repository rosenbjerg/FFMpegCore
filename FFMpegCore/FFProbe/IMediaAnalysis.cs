using System;
using System.Collections.Generic;

namespace FFMpegCore
{
    public interface IMediaAnalysis
    {
        string Path { get; }
        string Extension { get; }
        TimeSpan Duration { get; }
        MediaFormat Format { get; }
        AudioStream PrimaryAudioStream { get; }
        VideoStream PrimaryVideoStream { get; }
        List<VideoStream> VideoStreams { get; }
        List<AudioStream> AudioStreams { get; }
    }
}
