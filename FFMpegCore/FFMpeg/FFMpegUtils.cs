using System;
using System.Collections.Generic;
using System.Linq;
using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using FFMpegCore.Models;

namespace FFMpegCore
{
    public static class FFMpegUtils
    {
        #region PixelFormats
        internal static IReadOnlyList<PixelFormat> GetPixelFormatsInternal()
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);

            var list = new List<PixelFormat>();
            using var instance = new Instances.Instance(FFMpegOptions.Options.FFmpegBinary(), "-pix_fmts");
            instance.DataReceived += (e, args) =>
            {
                if (PixelFormat.TryParse(args.Data, out var fmt))
                    list.Add(fmt);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0) throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));

            return list.AsReadOnly();
        }

        public static IReadOnlyList<PixelFormat> GetPixelFormats()
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetPixelFormatsInternal();
            return FFMpegCache.PixelFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetPixelFormat(string name, out PixelFormat fmt)
        {
            if (!FFMpegOptions.Options.UseCache)
            {
                fmt = GetPixelFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return fmt != null;
            }
            else
                return FFMpegCache.PixelFormats.TryGetValue(name, out fmt);
        }

        public static PixelFormat GetPixelFormat(string name)
        {
            if (TryGetPixelFormat(name, out var fmt))
                return fmt;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Pixel format \"{name}\" not supported");
        }
        #endregion

        #region ContainerFormats
        internal static IReadOnlyList<ContainerFormat> GetContainersFormatsInternal()
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);

            var list = new List<ContainerFormat>();
            using var instance = new Instances.Instance(FFMpegOptions.Options.FFmpegBinary(), "-formats");
            instance.DataReceived += (e, args) =>
            {
                if (ContainerFormat.TryParse(args.Data, out var fmt))
                    list.Add(fmt);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0) throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));

            return list.AsReadOnly();
        }

        public static IReadOnlyList<ContainerFormat> GetContainerFormats()
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetContainersFormatsInternal();
            return FFMpegCache.ContainerFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetContainerFormat(string name, out ContainerFormat fmt)
        {
            if (!FFMpegOptions.Options.UseCache)
            {
                fmt = GetContainersFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return fmt != null;
            }
            else
                return FFMpegCache.ContainerFormats.TryGetValue(name, out fmt);
        }

        public static ContainerFormat GetContainerFormat(string name)
        {
            if (TryGetContainerFormat(name, out var fmt))
                return fmt;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Container format \"{name}\" not supported");
        }
        #endregion
        
        #region Codecs

        internal static void ParsePartOfCodecs(Dictionary<string, Codec> codecs, string arguments,
            Func<string, Codec?> parser)
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);

            using var instance = new Instances.Instance(FFMpegOptions.Options.FFmpegBinary(), arguments);
            instance.DataReceived += (e, args) =>
            {
                var codec = parser(args.Data);
                if (codec != null)
                    if (codecs.TryGetValue(codec.Name, out var parentCodec))
                        parentCodec.Merge(codec);
                    else
                        codecs.Add(codec.Name, codec);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0)
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));
        }

        internal static Dictionary<string, Codec> GetCodecsInternal()
        {
            var res = new Dictionary<string, Codec>();
            ParsePartOfCodecs(res, "-codecs", (s) =>
            {
                if (Codec.TryParseFromCodecs(s, out var codec))
                    return codec;
                return null;
            });
            ParsePartOfCodecs(res, "-encoders", (s) =>
            {
                if (Codec.TryParseFromEncodersDecoders(s, out var codec, true))
                    return codec;
                return null;
            });
            ParsePartOfCodecs(res, "-decoders", (s) =>
            {
                if (Codec.TryParseFromEncodersDecoders(s, out var codec, false))
                    return codec;
                return null;
            });

            return res;
        }

        public static IReadOnlyList<Codec> GetCodecs()
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetCodecsInternal().Values.ToList().AsReadOnly();
            return FFMpegCache.Codecs.Values.ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetCodecs(CodecType type)
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetCodecsInternal().Values.Where(x => x.Type == type).ToList().AsReadOnly();
            return FFMpegCache.Codecs.Values.Where(x => x.Type == type).ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetVideoCodecs() => GetCodecs(CodecType.Video);
        public static IReadOnlyList<Codec> GetAudioCodecs() => GetCodecs(CodecType.Audio);
        public static IReadOnlyList<Codec> GetSubtitleCodecs() => GetCodecs(CodecType.Subtitle);
        public static IReadOnlyList<Codec> GetDataCodecs() => GetCodecs(CodecType.Data);

        public static bool TryGetCodec(string name, out Codec codec)
        {
            if (!FFMpegOptions.Options.UseCache)
            {
                codec = GetCodecsInternal().Values.FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return codec != null;
            }
            else
                return FFMpegCache.Codecs.TryGetValue(name, out codec);
        }

        public static Codec GetCodec(string name)
        {
            if (TryGetCodec(name, out var codec) && codec != null)
                return codec;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{name}\" not supported");
        }

        #endregion
    }
}