using System.Text.RegularExpressions;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Enums
{
    public enum FeatureStatus
    {
        Unknown,
        NotSupported,
        Supported,
    }

    public class Codec
    {
        private static readonly Regex _codecsFormatRegex = new(@"([D\.])([E\.])([VASD\.])([I\.])([L\.])([S\.])\s+([a-z0-9_-]+)\s+(.+)");
        private static readonly Regex _decodersEncodersFormatRegex = new(@"([VASD\.])([F\.])([S\.])([X\.])([B\.])([D\.])\s+([a-z0-9_-]+)\s+(.+)");

        public class FeatureLevel
        {
            public bool IsExperimental { get; internal set; }
            public FeatureStatus SupportsFrameLevelMultithreading { get; internal set; }
            public FeatureStatus SupportsSliceLevelMultithreading { get; internal set; }
            public FeatureStatus SupportsDrawHorizBand { get; internal set; }
            public FeatureStatus SupportsDirectRendering { get; internal set; }

            internal void Merge(FeatureLevel other)
            {
                IsExperimental |= other.IsExperimental;
                SupportsFrameLevelMultithreading = (FeatureStatus)Math.Max((int)SupportsFrameLevelMultithreading, (int)other.SupportsFrameLevelMultithreading);
                SupportsSliceLevelMultithreading = (FeatureStatus)Math.Max((int)SupportsSliceLevelMultithreading, (int)other.SupportsSliceLevelMultithreading);
                SupportsDrawHorizBand = (FeatureStatus)Math.Max((int)SupportsDrawHorizBand, (int)other.SupportsDrawHorizBand);
                SupportsDirectRendering = (FeatureStatus)Math.Max((int)SupportsDirectRendering, (int)other.SupportsDirectRendering);
            }
        }

        public string Name { get; private set; }
        public CodecType Type { get; private set; }
        public bool DecodingSupported { get; private set; }
        public bool EncodingSupported { get; private set; }
        public bool IsIntraFrameOnly { get; private set; }
        public bool IsLossy { get; private set; }
        public bool IsLossless { get; private set; }
        public string Description { get; private set; } = null!;

        public FeatureLevel EncoderFeatureLevel { get; private set; }
        public FeatureLevel DecoderFeatureLevel { get; private set; }

        internal Codec(string name, CodecType type)
        {
            EncoderFeatureLevel = new FeatureLevel();
            DecoderFeatureLevel = new FeatureLevel();
            Name = name;
            Type = type;
        }

        internal static bool TryParseFromCodecs(string line, out Codec codec)
        {
            var match = _codecsFormatRegex.Match(line);
            if (!match.Success)
            {
                codec = null!;
                return false;
            }

            var name = match.Groups[7].Value;
            var type = match.Groups[3].Value switch
            {
                "V" => CodecType.Video,
                "A" => CodecType.Audio,
                "D" => CodecType.Data,
                "S" => CodecType.Subtitle,
                _ => CodecType.Unknown
            };

            if (type == CodecType.Unknown)
            {
                codec = null!;
                return false;
            }

            codec = new Codec(name, type);

            codec.DecodingSupported = match.Groups[1].Value != ".";
            codec.EncodingSupported = match.Groups[2].Value != ".";
            codec.IsIntraFrameOnly = match.Groups[4].Value != ".";
            codec.IsLossy = match.Groups[5].Value != ".";
            codec.IsLossless = match.Groups[6].Value != ".";
            codec.Description = match.Groups[8].Value;

            return true;
        }
        internal static bool TryParseFromEncodersDecoders(string line, out Codec codec, bool isEncoder)
        {
            var match = _decodersEncodersFormatRegex.Match(line);
            if (!match.Success)
            {
                codec = null!;
                return false;
            }

            var name = match.Groups[7].Value;
            var type = match.Groups[1].Value switch
            {
                "V" => CodecType.Video,
                "A" => CodecType.Audio,
                "D" => CodecType.Data,
                "S" => CodecType.Subtitle,
                _ => CodecType.Unknown
            };

            if (type == CodecType.Unknown)
            {
                codec = null!;
                return false;
            }

            codec = new Codec(name, type);

            var featureLevel = isEncoder ? codec.EncoderFeatureLevel : codec.DecoderFeatureLevel;

            codec.DecodingSupported = !isEncoder;
            codec.EncodingSupported = isEncoder;
            featureLevel.SupportsFrameLevelMultithreading = match.Groups[2].Value != "." ? FeatureStatus.Supported : FeatureStatus.NotSupported;
            featureLevel.SupportsSliceLevelMultithreading = match.Groups[3].Value != "." ? FeatureStatus.Supported : FeatureStatus.NotSupported;
            featureLevel.IsExperimental = match.Groups[4].Value != ".";
            featureLevel.SupportsDrawHorizBand = match.Groups[5].Value != "." ? FeatureStatus.Supported : FeatureStatus.NotSupported;
            featureLevel.SupportsDirectRendering = match.Groups[6].Value != "." ? FeatureStatus.Supported : FeatureStatus.NotSupported;
            codec.Description = match.Groups[8].Value;

            return true;
        }
        internal void Merge(Codec other)
        {
            if (Name != other.Name)
            {
                throw new FFMpegException(FFMpegExceptionType.Operation, "different codecs enable to merge");
            }

            Type |= other.Type;
            DecodingSupported |= other.DecodingSupported;
            EncodingSupported |= other.EncodingSupported;
            IsIntraFrameOnly |= other.IsIntraFrameOnly;
            IsLossy |= other.IsLossy;
            IsLossless |= other.IsLossless;

            EncoderFeatureLevel.Merge(other.EncoderFeatureLevel);
            DecoderFeatureLevel.Merge(other.DecoderFeatureLevel);

            if (Description != other.Description)
            {
                Description += "\r\n" + other.Description;
            }
        }
    }
}
