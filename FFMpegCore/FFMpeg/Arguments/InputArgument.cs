namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents input parameter
    /// </summary>
    public class InputArgument : IInputArgument
    {
        public readonly bool VerifyExists;
        public readonly string FilePath;

        public InputArgument(bool verifyExists, string filePaths)
        {
            VerifyExists = verifyExists;
            FilePath = filePaths;
        }

        public InputArgument(string path, bool verifyExists) : this(verifyExists, path) { }

        public void Pre()
        {
            if (VerifyExists && !File.Exists(FilePath))
            {
                throw new FileNotFoundException("Input file not found", FilePath);
            }
        }

        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Post() { }

        public string Text => $"-i \"{FilePath}\"";
    }
}
