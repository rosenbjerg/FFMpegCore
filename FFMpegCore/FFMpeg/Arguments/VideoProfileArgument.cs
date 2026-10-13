using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class VideoProfileArgument : IArgument
{
    public readonly VideoProfile Profile;

    public VideoProfileArgument(VideoProfile profile)
    {
        Profile = profile;
    }

    public string Text => $"-profile:v {Profile}";
}
