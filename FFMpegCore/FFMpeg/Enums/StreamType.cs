namespace FFMpegCore.Enums;

/// <summary>
///     https://ffmpeg.org/ffmpeg.html#Stream-specifiers-1
///     ’v’ or ’V’ for video, ’a’ for audio, ’s’ for subtitle, ’d’ for data, and ’t’ for attachments
///     ’V’ only matches video streams which are not attached pictures, video thumbnails or cover arts.
///     All for all types
/// </summary>
public enum StreamType
{
    Audio,
    Video,
    VideoNoAttachedPic,
    Subtitle,
    Data,
    Attachments,
    All
}

internal static class StreamTypeExtensions
{
    public static string Specifier(this StreamType streamType)
    {
        return streamType switch
        {
            StreamType.Audio => ":a",
            StreamType.Video => ":v",
            StreamType.VideoNoAttachedPic => ":V",
            StreamType.Subtitle => ":s",
            StreamType.Data => ":d",
            StreamType.Attachments => ":t",
            _ => string.Empty
        };
    }
}
