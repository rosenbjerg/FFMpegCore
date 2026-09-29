namespace FFMpegCore;

public interface IMediaAnalysis
{
    /// <summary>
    ///     The input this analysis describes, exactly as it was given to <see cref="FFProbe" /> — a file path, or the absolute
    ///     uri for a uri input. Null when the analysis came from a stream, which leaves nothing an ffmpeg run could reopen.
    /// </summary>
    string? Path { get; }

    TimeSpan Duration { get; }
    MediaFormat Format { get; }
    List<ChapterData> Chapters { get; }
    AudioStream? PrimaryAudioStream { get; }
    VideoStream? PrimaryVideoStream { get; }
    SubtitleStream? PrimarySubtitleStream { get; }
    List<VideoStream> VideoStreams { get; }
    List<AudioStream> AudioStreams { get; }
    List<SubtitleStream> SubtitleStreams { get; }
    IReadOnlyList<string> ErrorData { get; }
}
