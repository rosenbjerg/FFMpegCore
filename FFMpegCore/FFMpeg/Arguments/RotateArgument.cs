using System.Drawing;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class RotateArgument : IArgument
    {
        public readonly int Rotate;
        public RotateArgument(int arg)
        {
            Rotate = arg;
        }

        public string Text => Rotate switch
        {
           0 => $"",
           1 => $"-vf \"transpose=1\"",
           2 => $"-vf \"transpose=1, transpose=1\"",
           3 => $"-vf \"transpose=1, transpose=1, transpose=1\"",
           _ => throw new System.ArgumentOutOfRangeException(nameof(RotateArgument))
        };
    }
}
