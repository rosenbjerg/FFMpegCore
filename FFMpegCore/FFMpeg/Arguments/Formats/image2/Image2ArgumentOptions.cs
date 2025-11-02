namespace FFMpegCore.Arguments.Formats.image2;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#image2_002c-image2pipe" />
/// </summary>
public sealed class Image2ArgumentOptions
{
    private readonly FFMpegArgumentOptions _options;

    internal Image2ArgumentOptions(FFMpegArgumentOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// <inheritdoc cref="UpdateArgument"/>
    /// </summary>
    /// <param name="value"></param>
    public Image2ArgumentOptions WithUpdate(bool value = true)
    {
        return WithArgument(new UpdateArgument(value));
    }

    public Image2ArgumentOptions WithArgument(IImage2Argument argument)
    {
        _options.WithArgument(argument);
        return this;
    }
}
