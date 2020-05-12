using FFMpegCore.Enums;
using FFMpegCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFMpegCore
{
    static class FFMpegCache
    {
        private static readonly object _syncObject = new object();
        private static Dictionary<string, PixelFormat>? _pixelFormats;
        private static Dictionary<string, Codec>? _codecs;
        private static Dictionary<string, ContainerFormat>? _containers;

        public static IReadOnlyDictionary<string, PixelFormat> PixelFormats
        {
            get
            {
                if (_pixelFormats == null) //First check not thread safe
                    lock (_syncObject)
                        if (_pixelFormats == null)//Second check thread safe
                            _pixelFormats = FFMpeg.GetPixelFormatsInternal().ToDictionary(x => x.Name);

                return _pixelFormats;
            }

        }
        public static IReadOnlyDictionary<string, Codec> Codecs
        {
            get
            {
                if (_codecs == null) //First check not thread safe
                    lock (_syncObject)
                        if (_codecs == null)//Second check thread safe
                            _codecs = FFMpeg.GetCodecsInternal();

                return _codecs;
            }

        }
        public static IReadOnlyDictionary<string, ContainerFormat> ContainerFormats
        {
            get
            {
                if (_containers == null) //First check not thread safe
                    lock (_syncObject)
                        if (_containers == null)//Second check thread safe
                            _containers = FFMpeg.GetContainersFormatsInternal().ToDictionary(x => x.Name);

                return _containers;
            }

        }
    }
}
