namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class RotateArgument : IVideoFilterArgument
    {
        public readonly int Rotate;
        public RotateArgument(int arg)
        {
            Rotate = arg;
        }

        public string Key { get; } = "";
        public string Value => Rotate switch
        {
            0 => $"",
            1 => $"transpose=1",
            2 => $"transpose=1, transpose=1",
            3 => $"transpose=1, transpose=1, transpose=1",
            _ => throw new System.ArgumentOutOfRangeException(nameof(RotateArgument))
        };
    }
}
