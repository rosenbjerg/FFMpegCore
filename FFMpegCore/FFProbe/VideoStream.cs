using FFMpegCore.Enums;

namespace FFMpegCore;

public class VideoStream : MediaStream
{
    /// <summary>ffprobe's avg_frame_rate: total frames divided by total duration.</summary>
    public double AverageFrameRate { get; set; }
    public int BitsPerRawSample { get; set; }
    public (int Width, int Height) DisplayAspectRatio { get; set; }
    public (int Width, int Height) SampleAspectRatio { get; set; }
    public string Profile { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    /// <summary>ffprobe's r_frame_rate: the lowest rate every timestamp is a multiple of.</summary>
    public double RealFrameRate { get; set; }
    public string PixelFormat { get; set; } = null!;
    public int Level { get; set; }
    public string FieldOrder { get; set; } = null!;
    public int Rotation { get; set; }
    public string ColorRange { get; set; } = null!;
    public string ColorSpace { get; set; } = null!;
    public string ColorTransfer { get; set; } = null!;
    public string ColorPrimaries { get; set; } = null!;

    public PixelFormat GetPixelFormatInfo(FFOptions? ffOptions = null)
    {
        return FFMpeg.GetPixelFormat(PixelFormat, ffOptions);
    }
}
