using System;
using System.Collections.Generic;
using System.IO;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents parameter of concat argument
    /// Used for creating video from multiple images or videos
    /// </summary>
    public class DemuxConcatArgument : IInputArgument
    {
        public readonly IEnumerable<string> Values;
        public DemuxConcatArgument(IEnumerable<string> values)
        {
            Values = values;
        }
        private readonly string _tempFileName = Path.Combine(FFMpegOptions.Options.TempDirectory, Guid.NewGuid() + ".txt");

        public void Pre()
        {
            File.WriteAllLines(_tempFileName, Values);
        }

        public void Post()
        {
            File.Delete(_tempFileName);
        }

        public string Text => $"-f concat -safe 0 -i \"{_tempFileName}\"";
    }
}