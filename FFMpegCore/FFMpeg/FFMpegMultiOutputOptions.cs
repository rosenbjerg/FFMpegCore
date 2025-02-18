using FFMpegCore.Arguments;
using FFMpegCore.Pipes;

namespace FFMpegCore
{
    public class FFMpegMultiOutputOptions
    {
        internal readonly List<FFMpegArgumentOptions> Outputs = new();

        public IEnumerable<IArgument> Arguments => Outputs.SelectMany(o => o.Arguments);

        public FFMpegMultiOutputOptions OutputToFile(string file, bool overwrite = true, Action<FFMpegArgumentOptions>? addArguments = null) => AddOutput(new OutputArgument(file, overwrite), addArguments);

        public FFMpegMultiOutputOptions OutputToUrl(string uri, Action<FFMpegArgumentOptions>? addArguments = null) => AddOutput(new OutputUrlArgument(uri), addArguments);

        public FFMpegMultiOutputOptions OutputToUrl(Uri uri, Action<FFMpegArgumentOptions>? addArguments = null) => AddOutput(new OutputUrlArgument(uri.ToString()), addArguments);

        public FFMpegMultiOutputOptions OutputToPipe(IPipeSink reader, Action<FFMpegArgumentOptions>? addArguments = null) => AddOutput(new OutputPipeArgument(reader), addArguments);

        public FFMpegMultiOutputOptions AddOutput(IOutputArgument argument, Action<FFMpegArgumentOptions>? addArguments)
        {
            var args = new FFMpegArgumentOptions();
            addArguments?.Invoke(args);
            args.Arguments.Add(argument);
            Outputs.Add(args);
            return this;
        }
    }
}
