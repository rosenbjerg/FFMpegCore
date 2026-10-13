namespace FFMpegCore.Arguments;

public class AudioChannelsArgument : IArgument
{
    public readonly int Channels;

    public AudioChannelsArgument(int channels)
    {
        Channels = channels;
    }

    public string Text => $"-ac {Channels}";
}
