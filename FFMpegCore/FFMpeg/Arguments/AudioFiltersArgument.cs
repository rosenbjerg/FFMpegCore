using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

public class AudioFiltersArgument : IArgument
{
    public readonly AudioFilterOptions Options;

    public AudioFiltersArgument(AudioFilterOptions options)
    {
        Options = options;
    }

    public string Text => GetText();

    private string GetText()
    {
        if (!Options.Arguments.Any())
        {
            throw new FFMpegArgumentException("No audio-filter arguments provided");
        }

        var arguments = Options.Arguments
            .Where(arg => !string.IsNullOrEmpty(arg.Value))
            .Select(arg =>
            {
                var escapedValue = arg.Value.Replace(",", "\\,");
                return string.IsNullOrEmpty(arg.Key) ? escapedValue : $"{arg.Key}={escapedValue}";
            });

        return $"-af \"{string.Join(", ", arguments)}\"";
    }
}

public interface IAudioFilterArgument
{
    string Key { get; }
    string Value { get; }
}

public class AudioFilterOptions
{
    public List<IAudioFilterArgument> Arguments { get; } = new();

    /// <summary>pan</summary>
    public AudioFilterOptions Pan(string channelLayout, params string[] outputDefinitions)
    {
        return WithArgument(new PanArgument(channelLayout, outputDefinitions));
    }

    /// <summary>pan</summary>
    public AudioFilterOptions Pan(int channels, params string[] outputDefinitions)
    {
        return WithArgument(new PanArgument(channels, outputDefinitions));
    }

    /// <summary>dynaudnorm</summary>
    public AudioFilterOptions DynamicAudioNormalizer(int frameLength = 500, int filterWindow = 31, double targetPeak = 0.95,
        double gainFactor = 10.0, double targetRms = 0.0, bool channelCoupling = true,
        bool enableDcBiasCorrection = false, bool enableAlternativeBoundary = false,
        double compressorFactor = 0.0)
    {
        return WithArgument(new DynamicNormalizerArgument(frameLength, filterWindow,
            targetPeak, gainFactor, targetRms, channelCoupling, enableDcBiasCorrection, enableAlternativeBoundary,
            compressorFactor));
    }

    /// <summary>highpass</summary>
    public AudioFilterOptions HighPass(double frequency = 3000, int poles = 2, string widthType = "q", double width = 0.707,
        double mix = 1, string channels = "", bool normalize = false, string transform = "", string precision = "auto",
        int? blockSize = null)
    {
        return WithArgument(new HighPassFilterArgument(frequency, poles, widthType, width, mix, channels, normalize, transform, precision, blockSize));
    }

    /// <summary>lowpass</summary>
    public AudioFilterOptions LowPass(double frequency = 3000, int poles = 2, string widthType = "q", double width = 0.707,
        double mix = 1, string channels = "", bool normalize = false, string transform = "", string precision = "auto",
        int? blockSize = null)
    {
        return WithArgument(new LowPassFilterArgument(frequency, poles, widthType, width, mix, channels, normalize, transform, precision, blockSize));
    }

    /// <summary>agate</summary>
    public AudioFilterOptions AudioGate(double levelIn = 1, string mode = "downward", double range = 0.06125, double threshold = 0.125,
        int ratio = 2, double attack = 20, double release = 250, int makeup = 1, double knee = 2.828427125, string detection = "rms",
        string link = "average")
    {
        return WithArgument(new AudioGateArgument(levelIn, mode, range, threshold, ratio, attack, release, makeup, knee, detection, link));
    }

    /// <summary>silencedetect</summary>
    public AudioFilterOptions SilenceDetect(string noiseType = "db", double noise = 60, double duration = 2,
        bool mono = false)
    {
        return WithArgument(new SilenceDetectArgument(noiseType, noise, duration, mono));
    }

    private AudioFilterOptions WithArgument(IAudioFilterArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
