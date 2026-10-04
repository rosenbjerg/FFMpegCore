namespace FFMpegCore.Arguments;

public class MapMetadataArgument : IArgument
{
    public readonly int InputIndex;

    public MapMetadataArgument(int inputIndex)
    {
        InputIndex = inputIndex;
    }

    public string Text => $"-map_metadata {InputIndex}";
}
