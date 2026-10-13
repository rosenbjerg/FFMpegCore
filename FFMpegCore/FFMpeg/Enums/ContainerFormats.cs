namespace FFMpegCore.Enums;

public static class ContainerFormats
{
    public static ContainerFormat Ts => new("mpegts");
    public static ContainerFormat Mp4 => new("mp4");
    public static ContainerFormat Mov => new("mov");
    public static ContainerFormat Avi => new("avi");
    public static ContainerFormat Ogv => new("ogv");
    public static ContainerFormat WebM => new("webm");
    public static ContainerFormat Matroska => new("matroska");
    public static ContainerFormat Flv => new("flv");
    public static ContainerFormat Mp3 => new("mp3");
    public static ContainerFormat Wav => new("wav");
    public static ContainerFormat Flac => new("flac");
    public static ContainerFormat Hls => new("hls");
    public static ContainerFormat Image2 => new("image2");
    public static ContainerFormat RawVideo => new("rawvideo");
    public static ContainerFormat Null => new("null");
}
