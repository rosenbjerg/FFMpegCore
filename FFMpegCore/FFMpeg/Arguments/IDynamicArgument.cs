using System.Collections.Generic;

namespace FFMpegCore.Arguments
{
    public interface IDynamicArgument
    {
        /// <summary>
        /// Same as <see cref="IArgument.Text"/>, but this receives the arguments generated before as parameter
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        //public string GetText(StringBuilder context);
        public string GetText(IEnumerable<IArgument> context);
    }
}