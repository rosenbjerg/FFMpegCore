namespace FFMpegCore.Enums;

public static class ContainerFormats
{
    public static ContainerFormat Ts => new("mpegts");
    public static ContainerFormat Mp4 => new("mp4");
    public static ContainerFormat Mov => new("mov");
    public static ContainerFormat Avi => new("avi");
    public static ContainerFormat Ogv => new("ogv");
    public static ContainerFormat WebM => new("webm");
}
