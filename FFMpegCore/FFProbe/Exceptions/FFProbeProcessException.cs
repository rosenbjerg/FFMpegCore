namespace FFMpegCore.Exceptions
{
    public class FFProbeProcessException : FFProbeException
    {
        public IReadOnlyCollection<string> ProcessErrors { get; }

        public FFProbeProcessException(string message, IReadOnlyCollection<string> processErrors, Exception? inner = null) : base(message, inner)
        {
            ProcessErrors = processErrors;
        }
    }
}
