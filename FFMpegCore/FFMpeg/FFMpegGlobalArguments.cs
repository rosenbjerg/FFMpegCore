using FFMpegCore.Arguments;
using FFMpegCore.Arguments.GenericOptions;
using FFMpegCore.Arguments.MainOptions;
using FFMpegCore.Arguments.VideoOptions;
using FFMpegCore.Enums;

namespace FFMpegCore;

public sealed class FFMpegGlobalArguments : FFMpegArgumentsBase
{
    internal FFMpegGlobalArguments() { }

    public FFMpegGlobalArguments WithVerbosityLevel(VerbosityLevel verbosityLevel = VerbosityLevel.Error)
    {
        return WithArgument(new VerbosityLevelArgument(verbosityLevel));
    }

    /// <summary>
    /// <inheritdoc cref="HardwareAccelerationOutputFormatArgument"/>
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public FFMpegGlobalArguments WithHardwareAccelerationOutputFormat(HardwareAccelerationDevice device)
    {
        return WithArgument(new HardwareAccelerationOutputFormatArgument(device));
    }

    /// <summary>
    /// <inheritdoc cref="HideBanner"/>
    /// </summary>
    /// <returns></returns>
    public FFMpegGlobalArguments WithHideBanner()
    {
        return WithArgument(new HideBanner());
    }

    /// <summary>
    /// <inheritdoc cref="Stats"/>
    /// </summary>
    /// <returns></returns>
    public FFMpegGlobalArguments WithNoStats()
    {
        return WithArgument(new Stats(false));
    }

    public FFMpegGlobalArguments WithArgument(IArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
