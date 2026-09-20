using System.Drawing;

namespace FFMpegCore.Extensions.System.Drawing.Common;

public static class BitmapExtensions
{
    public static FFMpegResult AddAudio(this Image poster, string audio, string output)
    {
        var destination = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, $"{Guid.NewGuid()}.png");
        poster.Save(destination);
        try
        {
            return FFMpeg.PosterWithAudio(destination, audio, output).ProcessSynchronously();
        }
        finally
        {
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
        }
    }
}
