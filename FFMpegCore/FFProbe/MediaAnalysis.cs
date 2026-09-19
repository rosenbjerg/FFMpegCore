using FFMpegCore.Builders.MetaData;

namespace FFMpegCore;

internal class MediaAnalysis : IMediaAnalysis
{
    internal MediaAnalysis(FFProbeAnalysis analysis)
    {
        Format = ParseFormat(analysis.Format);
        Chapters = analysis.Chapters.Select(c => ParseChapter(c)).ToList();
        VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
        AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
        SubtitleStreams = analysis.Streams.Where(stream => stream.CodecType == "subtitle").Select(ParseSubtitleStream).ToList();
        ErrorData = analysis.ErrorData;
    }

    public TimeSpan Duration => new[] { Format.Duration, PrimaryVideoStream?.Duration ?? TimeSpan.Zero, PrimaryAudioStream?.Duration ?? TimeSpan.Zero }.Max();

    public MediaFormat Format { get; }

    public List<ChapterData> Chapters { get; }

    public AudioStream? PrimaryAudioStream => AudioStreams.OrderBy(stream => stream.Index).FirstOrDefault();
    public VideoStream? PrimaryVideoStream => VideoStreams.OrderBy(stream => stream.Index).FirstOrDefault();
    public SubtitleStream? PrimarySubtitleStream => SubtitleStreams.OrderBy(stream => stream.Index).FirstOrDefault();

    public List<VideoStream> VideoStreams { get; }
    public List<AudioStream> AudioStreams { get; }
    public List<SubtitleStream> SubtitleStreams { get; }
    public IReadOnlyList<string> ErrorData { get; }

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
            BitRate = long.Parse(analysisFormat.BitRate ?? "0"),
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
            AvgFrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.AvgFrameRate, '/')),
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            BitsPerRawSample = !string.IsNullOrEmpty(stream.BitsPerRawSample) ? MediaAnalysisUtils.ParseIntInvariant(stream.BitsPerRawSample) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            DisplayAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.DisplayAspectRatio, ':'),
            SampleAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.SampleAspectRatio, ':'),
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            FrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.FrameRate, '/')),
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
            BitDepth = GetBitDepth(stream)
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
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            SampleRateHz = !string.IsNullOrEmpty(stream.SampleRate) ? MediaAnalysisUtils.ParseIntInvariant(stream.SampleRate) : default,
            Profile = stream.Profile,
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            BitDepth = GetBitDepth(stream)
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
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive()
        };
    }
}
