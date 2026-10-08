namespace FFMpegCore;

public interface IMediaAnalysis
{
    /// <summary>
    ///     The input this analysis describes, as it was handed to ffprobe — a file path, the absolute uri for a uri input, or the
    ///     local path for a file uri. Null when the analysis came from a stream, which leaves nothing an ffmpeg run could reopen.
    /// </summary>
    string? Path { get; }

    string Json { get; }

    TimeSpan Duration { get; }
    MediaFormat Format { get; }
    IReadOnlyList<ChapterData> Chapters { get; }
    AudioStream? PrimaryAudioStream { get; }
    VideoStream? PrimaryVideoStream { get; }
    SubtitleStream? PrimarySubtitleStream { get; }
    IReadOnlyList<VideoStream> VideoStreams { get; }
    IReadOnlyList<AudioStream> AudioStreams { get; }
    IReadOnlyList<SubtitleStream> SubtitleStreams { get; }
    IReadOnlyList<string> StandardError { get; }
}
