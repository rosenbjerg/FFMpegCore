using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments.VideoFilters.Vaapi;

/// <summary>
/// <see href="https://www.ffmpeg.org/ffmpeg-filters.html#VAAPI-Video-Filters" />
/// </summary>
public sealed class VaapiVideoFilterOptions
{
    private readonly VideoFilterOptions _options;

    internal VaapiVideoFilterOptions(VideoFilterOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// <inheritdoc cref="VideoFilterScaleVaapiArgument"/>
    /// </summary>
    /// <param name="videosize"></param>
    /// <returns></returns>
    public VaapiVideoFilterOptions Scale(VideoSize videosize)
    {
        return WithArgument(new VideoFilterScaleVaapiArgument(videosize));
    }

    /// <summary>
    /// <inheritdoc cref="VideoFilterScaleVaapiArgument"/>
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public VaapiVideoFilterOptions Scale(int width, int height)
    {
        return WithArgument(new VideoFilterScaleVaapiArgument(width, height));
    }

    /// <summary>
    /// <inheritdoc cref="VideoFilterScaleVaapiArgument"/>
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public VaapiVideoFilterOptions Scale(Size size)
    {
        return WithArgument(new VideoFilterScaleVaapiArgument(size));
    }

    public VaapiVideoFilterOptions WithArgument(IVaapiVideoFilterArgument argument)
    {
        _options.Arguments.Add(argument);
        return this;
    }
}
