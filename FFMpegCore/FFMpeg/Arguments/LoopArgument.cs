namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents loop parameter
    /// </summary>
    public class LoopArgument : IArgument
    {
        public readonly int Times;
        public LoopArgument(int times)
        {
            Times = times;
        }

        public string Text => $"-loop {Times}";
    }
}
