namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Abstract class implements basic functionality of ffmpeg arguments
    /// </summary>
    public abstract class Argument
    {
        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public abstract string GetStringValue();

        public override string ToString()
        {
            return GetStringValue();
        }
    }

    /// <summary>
    /// Abstract class implements basic functionality of ffmpeg arguments with one value property
    /// </summary>
    public abstract class Argument<T> : Argument
    {
        /// <summary>
        /// Value type of <see cref="T"/>
        /// </summary>
        public T Value { get; protected set; }

        public Argument() { }

        public Argument(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Abstract class implements basic functionality of ffmpeg arguments with two values properties
    /// </summary>
    public abstract class Argument<T1, T2> : Argument
    {
        /// <summary>
        /// First value type of <see cref="T"/>
        /// </summary>
        public T1 First { get; set; }

        /// <summary>
        /// Second value type of <see cref="T"/>
        /// </summary>
        public T2 Second { get; set; }

        public Argument() { }

        public Argument(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }
}
