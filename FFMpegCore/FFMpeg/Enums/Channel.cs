namespace FFMpegCore.Enums;

/// <summary>
///     https://ffmpeg.org/ffmpeg.html#Stream-specifiers-1
///     ’v’ or ’V’ for video, ’a’ for audio, ’s’ for subtitle, ’d’ for data, and ’t’ for attachments
///     ’V’ only matches video streams which are not attached pictures, video thumbnails or cover arts.
///     Both for audio + video
///     All for all types
/// </summary>
public enum Channel
{
    Audio,
    Video,
    Both,
    VideoNoAttachedPic,
    Subtitle,
    Data,
    Attachments,
    All
}

internal static class ChannelMethods
{
    /// <summary>
    ///     <see cref="Channel.Both" /> is left as empty because it cannot be in a single stream specifier
    /// </summary>
    /// <returns>The stream_type used in stream specifiers</returns>
    public static string StreamType(this Channel channel)
    {
        return channel switch
        {
            Channel.Audio => ":a",
            Channel.Video => ":v",
            Channel.VideoNoAttachedPic => ":V",
            Channel.Subtitle => ":s",
            Channel.Data => ":d",
            Channel.Attachments => ":t",
            _ => string.Empty
        };
    }
}
