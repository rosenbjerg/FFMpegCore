using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents force format parameter
    /// </summary>
    public class ForceFormatArgument : IArgument
    {
        private readonly string _format;
        public ForceFormatArgument(string format)
        {
            _format = format;
        }

        public ForceFormatArgument(ContainerFormat format)
        {
            _format = format.Name;
        }

        public string Text => $"-f {_format}";
    }
}
