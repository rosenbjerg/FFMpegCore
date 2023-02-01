namespace FFMpegCore.Arguments
{
    public class CustomArgument : IArgument
    {
        public readonly string Argument;

        public CustomArgument(string argument)
        {
            Argument = argument;
        }

        public string Text => Argument ?? string.Empty;
    }
}
