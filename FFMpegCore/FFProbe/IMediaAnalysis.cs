namespace FFMpegCore;

public interface IMediaAnalysis
{
    /// <summary>
    ///     The input this analysis describes, exactly as it was given to <see cref="FFProbe" /> — a file path, or the absolute
    ///     uri for a uri input. Null when the analysis came from a stream, which leaves nothing an ffmpeg run could reopen.
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
