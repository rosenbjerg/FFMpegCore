using System.Drawing;

namespace FFMpegCore.Test;

[TestClass]
public class SnapshotArgumentBuilderTests
{
    private sealed class VideoOnlyAnalysis : IMediaAnalysis
    {
        public VideoOnlyAnalysis(int width, int height, int rotation)
        {
            PrimaryVideoStream = new VideoStream { Index = 0, Width = width, Height = height, Rotation = rotation };
            VideoStreams = new List<VideoStream> { PrimaryVideoStream };
        }

        public TimeSpan Duration => TimeSpan.FromSeconds(3);
        public MediaFormat Format => new();
        public List<ChapterData> Chapters => new();
        public AudioStream PrimaryAudioStream => null;
        public VideoStream PrimaryVideoStream { get; }
        public SubtitleStream PrimarySubtitleStream => null;
        public List<VideoStream> VideoStreams { get; }
        public List<AudioStream> AudioStreams => new();
        public List<SubtitleStream> SubtitleStreams => new();
        public IReadOnlyList<string> ErrorData => Array.Empty<string>();
    }

    private static string SnapshotArguments(IMediaAnalysis source, Size? size)
    {
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments("input.mp4", "output.png", source, size);
        return arguments.OutputToFile("output.png", true, outputOptions).Arguments;
    }

    [TestMethod]
    [DataRow(0, 360, 0, "scale=360:202")]
    [DataRow(180, 360, 0, "scale=360:202")]
    [DataRow(-180, 360, 0, "scale=360:202")]
    [DataRow(90, 360, 0, "scale=360:640")]
    [DataRow(-90, 360, 0, "scale=360:640")]
    [DataRow(0, 0, 360, "scale=640:360")]
    [DataRow(90, 0, 360, "scale=202:360")]
    [DataRow(0, 640, 480, "scale=640:480")]
    public void Snapshot_ScalesAgainstTheDisplayedSize(int rotation, int wantedWidth, int wantedHeight, string expectedScale)
    {
        var source = new VideoOnlyAnalysis(1280, 720, rotation);

        Assert.Contains(expectedScale, SnapshotArguments(source, new Size(wantedWidth, wantedHeight)));
    }

    [TestMethod]
    [DataRow(0, 1280, 720)]
    [DataRow(90, 720, 1280)]
    public void Snapshot_SizeMatchingTheSource_AddsNoScaleFilter(int rotation, int wantedWidth, int wantedHeight)
    {
        var source = new VideoOnlyAnalysis(1280, 720, rotation);

        Assert.DoesNotContain("scale", SnapshotArguments(source, new Size(wantedWidth, wantedHeight)));
    }

    [TestMethod]
    public void Snapshot_WithoutASize_AddsNoScaleFilter()
    {
        Assert.DoesNotContain("scale", SnapshotArguments(new VideoOnlyAnalysis(1280, 720, 0), null));
    }
}
