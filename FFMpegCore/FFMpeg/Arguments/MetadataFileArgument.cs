namespace FFMpegCore.Arguments;

public class MetadataFileArgument : InputArgument
{
    public MetadataFileArgument(string filePath, bool verifyExists) : base(filePath, verifyExists) { }
}
