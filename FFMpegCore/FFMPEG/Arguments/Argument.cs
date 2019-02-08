using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
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
        private T _value;

        /// <summary>
        /// Value type of <see cref="T"/>
        /// </summary>
        public T Value { get => _value; set { CheckValue(value); _value = value; } }

        public Argument() { }

        public Argument(T value)
        {
            Value = value;
        }

        protected virtual void CheckValue(T value)
        {
            
        }
    }

    /// <summary>
    /// Abstract class implements basic functionality of ffmpeg arguments with two values properties
    /// </summary>
    public abstract class Argument<T1, T2> : Argument
    {

        private T1 _first;
        private T2 _second;

        /// <summary>
        /// First value type of <see cref="T"/>
        /// </summary>
        public T1 First { get => _first; set { CheckFirst(_first); _first = value; } }

        /// <summary>
        /// Second value type of <see cref="T"/>
        /// </summary>
        public T2 Second { get => _second; set { CheckSecond(_second); _second = value; } }

        public Argument() { }

        public Argument(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        protected virtual void CheckFirst(T1 value)
        {

        }

        protected virtual void CheckSecond(T2 value)
        {

        }
    }
}
