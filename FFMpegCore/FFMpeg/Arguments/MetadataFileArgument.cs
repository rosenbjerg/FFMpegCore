namespace FFMpegCore.Arguments;

internal class MetadataFileArgument : InputArgument
{
    public MetadataFileArgument(string filePath, bool verifyExists) : base(filePath, verifyExists) { }
}
