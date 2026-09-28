namespace FFMpegCore.Arguments;

public class Id3v2VersionArgument : IArgument
{
    private readonly int _version;

    public Id3v2VersionArgument(int version)
    {
        _version = version;
    }

    public string Text => $"-id3v2_version {_version}";
}
