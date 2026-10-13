namespace FFMpegCore.Enums;

public readonly struct FadeCurve
{
    private FadeCurve(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static FadeCurve Triangular => new("tri");
    public static FadeCurve QuarterSine => new("qsin");
    public static FadeCurve HalfSine => new("hsin");
    public static FadeCurve ExponentialSine => new("esin");
    public static FadeCurve Logarithmic => new("log");
    public static FadeCurve InvertedParabola => new("ipar");
    public static FadeCurve Quadratic => new("qua");
    public static FadeCurve Cubic => new("cub");
    public static FadeCurve SquareRoot => new("squ");
    public static FadeCurve CubicRoot => new("cbr");
    public static FadeCurve Parabola => new("par");
    public static FadeCurve Exponential => new("exp");
    public static FadeCurve InvertedQuarterSine => new("iqsin");
    public static FadeCurve InvertedHalfSine => new("ihsin");
    public static FadeCurve DoubleExponentialSeat => new("dese");
    public static FadeCurve DoubleExponentialSigmoid => new("desi");
    public static FadeCurve LogisticSigmoid => new("losi");
    public static FadeCurve SineCardinal => new("sinc");
    public static FadeCurve InvertedSineCardinal => new("isinc");
    public static FadeCurve NoFade => new("nofade");

    public static implicit operator FadeCurve(string value)
    {
        return new FadeCurve(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
