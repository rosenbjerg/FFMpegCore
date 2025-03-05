using FFMpegCore.Builders.MetaData;

namespace FFMpegCore
{
    public interface IMediaAnalysis
    {
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
}
