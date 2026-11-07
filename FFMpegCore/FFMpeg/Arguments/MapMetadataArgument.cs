namespace FFMpegCore.Arguments;

public class MapMetadataArgument : IArgument
{
    public readonly int InputFileIndex;

    public MapMetadataArgument(int inputFileIndex)
    {
        InputFileIndex = inputFileIndex;
    }

    public string Text => $"-map_metadata {InputFileIndex}";
}
