namespace FFMpegCore.Arguments;

/// <summary>
///     Represents input parameters for multiple files
/// </summary>
public class MultiInputArgument : IInputArgument
{
    public readonly IEnumerable<string> FilePaths;
    public readonly bool VerifyExists;

    public MultiInputArgument(bool verifyExists, IEnumerable<string> filePaths)
    {
        VerifyExists = verifyExists;
        FilePaths = filePaths;
    }

    public MultiInputArgument(IEnumerable<string> filePaths, bool verifyExists) : this(verifyExists, filePaths) { }

    public void Pre()
    {
        if (VerifyExists)
        {
            var missingFiles = new List<string>();
            foreach (var filePath in FilePaths)
            {
                if (!File.Exists(filePath))
                {
                    missingFiles.Add(filePath);
                }
            }

            if (missingFiles.Any())
            {
                throw new FileNotFoundException($"The following input files were not found: {string.Join(", ", missingFiles)}");
            }
        }
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post() { }

    /// <summary>
    ///     Generates a combined input argument text for all file paths
    /// </summary>
    public string Text => string.Join(" ", FilePaths.Select(filePath => $"-i \"{filePath}\""));
}
