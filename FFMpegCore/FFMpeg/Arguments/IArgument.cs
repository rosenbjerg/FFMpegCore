namespace FFMpegCore.Arguments
{
    public interface IArgument
    {
        /// <summary>
        /// The textual representation of the argument
        /// </summary>
        string Text { get; }
    }
}