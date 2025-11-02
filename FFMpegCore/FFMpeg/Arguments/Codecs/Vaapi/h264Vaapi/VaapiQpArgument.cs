namespace FFMpegCore.Arguments.Codecs.Vaapi.h264Vaapi;

/// <summary>
/// undocumented(?) <see href="https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/vaapi_encode_h264.c#L1073"/>
/// Constant QP (for P-frames; scaled by qfactor/qoffset for I/B)
/// </summary>
/// <param name="quantizer"><c>0</c> - <c>52</c></param>
public sealed class VaapiQpArgument(sbyte quantizer) : IH264VaapiArgument
{
    public string Text => $"-qp {quantizer}";
}
