using FFMpegCore.Models;

namespace FFMpegCore.Utils
{
    public static class VideoType
    {
        public static ContainerFormat MpegTs => FFMpegUtils.GetContainerFormat("mpegts");
        public static ContainerFormat Ts => FFMpegUtils.GetContainerFormat("mpegts");
        public static ContainerFormat Mp4 => FFMpegUtils.GetContainerFormat("mp4");
        public static ContainerFormat Mov => FFMpegUtils.GetContainerFormat("mov");
        public static ContainerFormat Avi => FFMpegUtils.GetContainerFormat("avi");
        public static ContainerFormat Ogv => FFMpegUtils.GetContainerFormat("ogv");
        public static ContainerFormat WebM => FFMpegUtils.GetContainerFormat("webm");
    }
}