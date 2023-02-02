namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents video bitrate parameter
    /// </summary>
    public class VideoBitrateArgument : IArgument
    {
        public readonly int Bitrate;

        public VideoBitrateArgument(int bitrate)
        {
            Bitrate = bitrate;
        }

        public string Text => $"-b:v {Bitrate}k";
    }
}
