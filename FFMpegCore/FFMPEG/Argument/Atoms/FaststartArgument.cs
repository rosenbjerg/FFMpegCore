namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Faststart argument - for moving moov atom to the start of file
    /// </summary>
    public class FaststartArgument : Argument
    {
        public FaststartArgument() { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return "-movflags faststart";
        }
    }
}