using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    public class SetMirroringArgument : IVideoFilterArgument
    {
        public SetMirroringArgument(Mirroring mirroring)
        {
            Mirroring = mirroring;
        }

        public Mirroring Mirroring { get; set; }

        public string Key => string.Empty;

        public string Value =>
            Mirroring switch
            {
                Mirroring.Horizontal => "hflip",
                Mirroring.Vertical => "vflip",
                _ => throw new ArgumentOutOfRangeException(nameof(Mirroring))
            };
    }
}
