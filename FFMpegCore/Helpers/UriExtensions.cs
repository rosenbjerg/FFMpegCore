namespace FFMpegCore.Helpers;

internal static class UriExtensions
{
    // ffmpeg opens file:///D:/x as the path ///D:/x, which only resolves on Unix
    public static string ToFFMpegUrl(this Uri uri)
    {
        return uri.IsFile ? uri.LocalPath : uri.AbsoluteUri;
    }
}
