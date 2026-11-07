using FFMpegCore.Enums;

namespace FFMpegCore;

public class VideoStream : MediaStream
{
    internal VideoStream() { }

    /// <summary>ffprobe's avg_frame_rate: total frames divided by total duration.</summary>
    public double AverageFrameRate { get; internal set; }
    public int? BitsPerRawSample { get; internal set; }
    public (int Width, int Height) DisplayAspectRatio { get; internal set; }
    public (int Width, int Height) SampleAspectRatio { get; internal set; }
    public string? Profile { get; internal set; }
    public int? Width { get; internal set; }
    public int? Height { get; internal set; }
    /// <summary>ffprobe's r_frame_rate: the lowest rate every timestamp is a multiple of.</summary>
    public double RealFrameRate { get; internal set; }
    public string? PixelFormat { get; internal set; }
    public int Level { get; internal set; }
    public string? FieldOrder { get; internal set; }
    public int Rotation { get; internal set; }
    public string? ColorRange { get; internal set; }
    public string? ColorSpace { get; internal set; }
    public string? ColorTransfer { get; internal set; }
    public string? ColorPrimaries { get; internal set; }

    public PixelFormat GetPixelFormatInfo(FFOptions? ffOptions = null)
    {
        return FFMpeg.GetPixelFormat(PixelFormat ?? throw new InvalidOperationException($"ffprobe reported no pixel format for stream {Index}"), ffOptions);
    }
}
