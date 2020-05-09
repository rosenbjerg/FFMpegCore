namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Faststart argument - for moving moov atom to the start of file
    /// </summary>
    public class FaststartArgument : IArgument
    {
        public string Text => "-movflags faststart";
    }
}