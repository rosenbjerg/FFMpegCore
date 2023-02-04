namespace FFMpegCore.Arguments
{

    /// <summary>
    /// Represents parameter of concat argument
    /// Used for creating video from multiple images or videos
    /// </summary>
    public class ConcatArgument : IInputArgument
    {
        public readonly IEnumerable<string> Values;
        public ConcatArgument(IEnumerable<string> values)
        {
            Values = values;
        }

        public void Pre() { }
        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Post() { }

        public string Text => $"-i \"concat:{string.Join(@"|", Values)}\"";
    }
}
