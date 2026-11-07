using System.Globalization;

namespace FFMpegCore.Arguments;

public class FrameRateArgument : IArgument
{
    public readonly string FrameRate;

    public FrameRateArgument(double frameRate) : this(frameRate.ToString(CultureInfo.InvariantCulture)) { }

    public FrameRateArgument(string frameRate)
    {
        FrameRate = frameRate;
    }

    public string Text => $"-r {FrameRate}";
}
