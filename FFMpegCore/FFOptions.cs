using System.Text;
using System.Text.Json.Serialization;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class FFOptions : ICloneable
{
    /// <summary>
    ///     Working directory for the ffmpeg/ffprobe instance
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    ///     Folder container ffmpeg and ffprobe binaries. Leave empty if ffmpeg and ffprobe are present in PATH
    /// </summary>
    public string BinaryFolder { get; set; } = string.Empty;

    /// <summary>
    ///     Folder used for temporary files necessary for static methods on FFMpeg class
    /// </summary>
    public string TemporaryFilesFolder { get; set; } = Path.GetTempPath();

    /// <summary>
    ///     Encoding web name used to persist encoding <see cref="Encoding" />
    /// </summary>
    public string EncodingWebName { get; set; } = Encoding.Default.WebName;

    /// <summary>
    ///     Encoding used for parsing stdout/stderr on ffmpeg and ffprobe processes
    /// </summary>
    [JsonIgnore]
    public Encoding Encoding
    {
        get => Encoding.GetEncoding(EncodingWebName);
        set => EncodingWebName = value?.WebName ?? Encoding.Default.WebName;
    }

    /// <summary>
    ///     The log level to use when calling of the ffmpeg executable.
    ///     <para>
    ///         This option can be overridden before an execution of a Process command
    ///         to set the log level for that command.
    ///     </para>
    /// </summary>
    public FFMpegLogLevel? LogLevel { get; set; }

    /// <summary>
    /// </summary>
    public Dictionary<string, string> ExtensionOverrides { get; set; } = new() { { "mpegts", ".ts" } };

    /// <summary>
    ///     Whether to cache calls to get ffmpeg codec, pixel- and container-formats
    /// </summary>
    public bool UseCache { get; set; } = true;

    /// <inheritdoc />
    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <summary>
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    public FFOptions Clone()
    {
        return (FFOptions)MemberwiseClone();
    }
}
