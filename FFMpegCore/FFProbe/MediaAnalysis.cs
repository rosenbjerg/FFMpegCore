namespace FFMpegCore;

internal class MediaAnalysis : IMediaAnalysis
{
    internal MediaAnalysis(FFProbeAnalysis analysis, string? path)
    {
        Path = path;
        Format = ParseFormat(analysis.Format);
        Chapters = analysis.Chapters.Select(c => ParseChapter(c)).ToList();
        VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
        AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
        SubtitleStreams = analysis.Streams.Where(stream => stream.CodecType == "subtitle").Select(ParseSubtitleStream).ToList();
        StandardError = analysis.StandardError;
    }

    public string? Path { get; }

    public TimeSpan Duration => new[] { Format.Duration, PrimaryVideoStream?.Duration ?? TimeSpan.Zero, PrimaryAudioStream?.Duration ?? TimeSpan.Zero }.Max();

    public MediaFormat Format { get; }

    public IReadOnlyList<ChapterData> Chapters { get; }

    public AudioStream? PrimaryAudioStream => Primary(AudioStreams);
    public VideoStream? PrimaryVideoStream => Primary(VideoStreams);
    public SubtitleStream? PrimarySubtitleStream => Primary(SubtitleStreams);

    public IReadOnlyList<VideoStream> VideoStreams { get; }
    public IReadOnlyList<AudioStream> AudioStreams { get; }
    public IReadOnlyList<SubtitleStream> SubtitleStreams { get; }
    public IReadOnlyList<string> StandardError { get; }

    private static T? Primary<T>(IEnumerable<T> streams) where T : MediaStream
    {
        return streams
            .OrderByDescending(stream => stream.Disposition != null && stream.Disposition.TryGetValue("default", out var isDefault) && isDefault)
            .ThenBy(stream => stream.Index)
            .FirstOrDefault();
    }

    private MediaFormat ParseFormat(Format analysisFormat)
    {
        return new MediaFormat
        {
            Duration = MediaAnalysisUtils.ParseDuration(analysisFormat.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(analysisFormat.StartTime),
            FormatName = analysisFormat.FormatName,
            FormatLongName = analysisFormat.FormatLongName,
            StreamCount = analysisFormat.NbStreams,
            ProbeScore = analysisFormat.ProbeScore,
            BitRate = !string.IsNullOrEmpty(analysisFormat.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(analysisFormat.BitRate!) : default,
            Tags = analysisFormat.Tags.ToCaseInsensitive()
        };
    }

    private string GetValue(string tagName, Dictionary<string, string>? tags, string defaultValue)
    {
        return tags == null ? defaultValue : tags.TryGetValue(tagName, out var value) ? value : defaultValue;
    }

    private ChapterData ParseChapter(Chapter analysisChapter)
    {
        var title = GetValue("title", analysisChapter.Tags, "TitleValueNotSet");
        var start = MediaAnalysisUtils.ParseDuration(analysisChapter.StartTime);
        var end = MediaAnalysisUtils.ParseDuration(analysisChapter.EndTime);

        return new ChapterData(title, start, end);
    }

    private static TimeSpan ParseStreamDuration(FFProbeStream stream)
    {
        var duration = string.IsNullOrEmpty(stream.Duration) ? stream.GetDuration() : stream.Duration;
        return MediaAnalysisUtils.ParseDuration(duration ?? string.Empty);
    }

    private int? GetBitDepth(FFProbeStream stream)
    {
        var bitDepth = int.TryParse(stream.BitsPerRawSample, out var bprs) ? bprs : stream.BitsPerSample;
        return bitDepth == 0 ? null : bitDepth;
    }

    private VideoStream ParseVideoStream(FFProbeStream stream)
    {
        return new VideoStream
        {
            Index = stream.Index,
            AverageFrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.AvgFrameRate, '/')),
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            BitsPerRawSample = !string.IsNullOrEmpty(stream.BitsPerRawSample) ? MediaAnalysisUtils.ParseIntInvariant(stream.BitsPerRawSample) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            DisplayAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.DisplayAspectRatio, ':'),
            SampleAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.SampleAspectRatio, ':'),
            Duration = ParseStreamDuration(stream),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            RealFrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.FrameRate, '/')),
            Height = stream.Height ?? 0,
            Width = stream.Width ?? 0,
            Profile = stream.Profile,
            PixelFormat = stream.PixelFormat,
            Level = stream.Level,
            FieldOrder = stream.FieldOrder,
            ColorRange = stream.ColorRange,
            ColorSpace = stream.ColorSpace,
            ColorTransfer = stream.ColorTransfer,
            ColorPrimaries = stream.ColorPrimaries,
            Rotation = MediaAnalysisUtils.ParseRotation(stream),
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            BitDepth = GetBitDepth(stream),
            SideData = stream.SideData
        };
    }

    private AudioStream ParseAudioStream(FFProbeStream stream)
    {
        return new AudioStream
        {
            Index = stream.Index,
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            Channels = stream.Channels ?? default,
            ChannelLayout = stream.ChannelLayout,
            Duration = ParseStreamDuration(stream),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            SampleRateHz = !string.IsNullOrEmpty(stream.SampleRate) ? MediaAnalysisUtils.ParseIntInvariant(stream.SampleRate) : default,
            Profile = stream.Profile,
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            BitDepth = GetBitDepth(stream),
            SideData = stream.SideData
        };
    }

    private SubtitleStream ParseSubtitleStream(FFProbeStream stream)
    {
        return new SubtitleStream
        {
            Index = stream.Index,
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            Duration = ParseStreamDuration(stream),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            SideData = stream.SideData
        };
    }
}
