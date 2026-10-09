using System.Globalization;

namespace FFMpegCore.Arguments;

public class ConstantRateFactorArgument : IArgument
{
    public readonly double Crf;

    public ConstantRateFactorArgument(double crf)
    {
        if (crf < 0 || crf > 63)
        {
            throw new ArgumentException("Argument is outside range (0 - 63)", nameof(crf));
        }

        Crf = crf;
    }

    public string Text => $"-crf {Crf.ToString(CultureInfo.InvariantCulture)}";
}
