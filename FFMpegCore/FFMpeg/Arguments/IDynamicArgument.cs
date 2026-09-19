namespace FFMpegCore.Arguments;

public interface IDynamicArgument
{
    /// <summary>
    ///     Same as <see cref="IArgument.Text" />, but this receives the arguments generated before as parameter
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    string GetText(IEnumerable<IArgument> context);
}
