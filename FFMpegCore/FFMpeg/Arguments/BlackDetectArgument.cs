namespace FFMpegCore.Arguments
{
    public class BlackDetectArgument : IVideoFilterArgument
    {
        public string Key => "blackdetect";

        public string Value { get; }

        public BlackDetectArgument(double minimumDuration = 2.0, double pictureBlackRatioThreshold = 0.98, double pixelBlackThreshold = 0.1)
        {
            Value = $"d={minimumDuration}:pic_th={pictureBlackRatioThreshold}:pix_th={pixelBlackThreshold}";
        }
    }
}
