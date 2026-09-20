namespace FFMpegCore.Enums;

public static class VideoType
{
    public static ContainerFormat MpegTs => FFMpeg.GetContainerFormat("mpegts");
    public static ContainerFormat Ts => FFMpeg.GetContainerFormat("mpegts");
    public static ContainerFormat Mp4 => FFMpeg.GetContainerFormat("mp4");
    public static ContainerFormat Mov => FFMpeg.GetContainerFormat("mov");
    public static ContainerFormat Avi => FFMpeg.GetContainerFormat("avi");
    public static ContainerFormat Ogv => FFMpeg.GetContainerFormat("ogv");
    public static ContainerFormat WebM => FFMpeg.GetContainerFormat("webm");
}
