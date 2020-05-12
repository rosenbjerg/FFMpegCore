using FFMpegCore.Models;

namespace FFMpegCore
{
    public abstract class MediaStream : SimpleStream
    {
        public int BitRate { get; internal set; }

        public Codec GetCodecInfo() => FFMpegUtils.GetCodec(CodecName);
        
    }
}