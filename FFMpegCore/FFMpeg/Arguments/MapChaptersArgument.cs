namespace FFMpegCore.Arguments;

public class MapChaptersArgument : IArgument
{
    public readonly int InputFileIndex;

    public MapChaptersArgument(int inputFileIndex)
    {
        InputFileIndex = inputFileIndex;
    }

    public string Text => $"-map_chapters {InputFileIndex}";
}
