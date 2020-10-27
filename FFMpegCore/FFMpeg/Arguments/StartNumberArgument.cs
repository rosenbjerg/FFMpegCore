namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents start number parameter
    /// </summary>
    public class StartNumberArgument : IArgument
    {
        public readonly int StartNumber;
        
        public StartNumberArgument(int startNumber)
        {
            StartNumber = startNumber;
        }

        public string Text => $"-start_number {StartNumber}";
    }
}
