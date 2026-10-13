namespace FFMpegCore.Enums;

public readonly struct EncoderTune
{
    private EncoderTune(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static EncoderTune Film => new("film");
    public static EncoderTune Animation => new("animation");
    public static EncoderTune Grain => new("grain");
    public static EncoderTune StillImage => new("stillimage");
    public static EncoderTune FastDecode => new("fastdecode");
    public static EncoderTune ZeroLatency => new("zerolatency");
    public static EncoderTune Psnr => new("psnr");
    public static EncoderTune Ssim => new("ssim");

    public static implicit operator EncoderTune(string value)
    {
        return new EncoderTune(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
