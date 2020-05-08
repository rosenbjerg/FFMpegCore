﻿using System.Collections.Generic;
using System.Linq;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Drawtext video filter argument
    /// </summary>
    public class DrawTextArgument : IArgument
    {
        public readonly DrawTextOptions Options;

        public DrawTextArgument(string text, string fontPath, params (string, string)[] optionalArguments) 
            : this(DrawTextOptions.Create(text, fontPath, optionalArguments)) { }
        
        public DrawTextArgument(DrawTextOptions options)
        {
            Options = options;
        }
        
        public string Text => $"-vf drawtext=\"{Options.TextInternal}\"";
    }
    
    public class DrawTextOptions
    {
        public readonly string Text;
        public readonly string Font;
        public readonly List<(string key, string value)> Parameters;

        public static DrawTextOptions Create(string text, string font)
        {
            return new DrawTextOptions(text, font, new List<(string, string)>());
        }
        public static DrawTextOptions Create(string text, string font, IEnumerable<(string key, string value)> parameters)
        {
            return new DrawTextOptions(text, font, parameters);
        }

        internal string TextInternal => string.Join(":", new[] {("text", Text), ("fontfile", Font)}.Concat(Parameters).Select(FormatArgumentPair));

        private static string FormatArgumentPair((string key, string value) pair)
        {
            return $"{pair.key}={EncloseIfContainsSpace(pair.value)}";
        }

        private static string EncloseIfContainsSpace(string input)
        {
            return input.Contains(" ") ? $"'{input}'" : input;
        }

        private DrawTextOptions(string text, string font, IEnumerable<(string, string)> parameters)
        {
            Text = text;
            Font = font;
            Parameters = parameters.ToList();
        }

        public DrawTextOptions WithParameter(string key, string value)
        {
            Parameters.Add((key, value));
            return this;
        }
    }
}