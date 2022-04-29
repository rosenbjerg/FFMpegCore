namespace FFMpegCore.Arguments
{
    public class ReadRateArgument : IArgument
    {
        private readonly double _readSpeed;

        public ReadRateArgument(double readSpeed)
        {
            _readSpeed = readSpeed;
        }
        public string Text => $"-readrate {_readSpeed} ";
    }
}