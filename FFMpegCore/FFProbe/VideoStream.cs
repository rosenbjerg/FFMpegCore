using FFMpegCore.Enums;

namespace FFMpegCore;

public class VideoStream : MediaStream
{
    internal VideoStream() { }

    /// <summary>ffprobe's avg_frame_rate: total frames divided by total duration.</summary>
    public double AverageFrameRate { get; internal set; }
    public int BitsPerRawSample { get; internal set; }
    public (int Width, int Height) DisplayAspectRatio { get; internal set; }
    public (int Width, int Height) SampleAspectRatio { get; internal set; }
    public string Profile { get; internal set; } = null!;
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    /// <summary>ffprobe's r_frame_rate: the lowest rate every timestamp is a multiple of.</summary>
    public double RealFrameRate { get; internal set; }
    public string PixelFormat { get; internal set; } = null!;
    public int Level { get; internal set; }
    public string FieldOrder { get; internal set; } = null!;
    public int Rotation { get; internal set; }
    public string ColorRange { get; internal set; } = null!;
    public string ColorSpace { get; internal set; } = null!;
    public string ColorTransfer { get; internal set; } = null!;
    public string ColorPrimaries { get; internal set; } = null!;

    public PixelFormat GetPixelFormatInfo(FFOptions? ffOptions = null)
    {
        return FFMpeg.GetPixelFormat(PixelFormat, ffOptions);
    }
}
