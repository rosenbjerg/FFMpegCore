namespace FFMpegCore.Arguments
{
    public class ID3V2VersionArgument : IArgument
    {
        private readonly int _version;

        public ID3V2VersionArgument(int version)
        {
            _version = version;
        }

        public string Text => $"-id3v2_version {_version}";
    }
}
