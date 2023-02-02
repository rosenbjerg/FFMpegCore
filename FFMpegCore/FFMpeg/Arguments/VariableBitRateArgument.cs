namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Variable Bitrate Argument (VBR) argument
    /// </summary>
    public class VariableBitRateArgument : IArgument
    {
        public readonly int Vbr;

        public VariableBitRateArgument(int vbr)
        {
            if (vbr < 0 || vbr > 5)
            {
                throw new ArgumentException("Argument is outside range (0 - 5)", nameof(vbr));
            }

            Vbr = vbr;
        }

        public string Text => $"-vbr {Vbr}";
    }
}
