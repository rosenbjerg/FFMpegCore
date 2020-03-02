using System;
using System.Collections;
using System.Collections.Generic;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Container of arguments represented parameters of FFMPEG process
    /// </summary>
    public class ArgumentContainer : IDictionary<Type, Argument>
    {
        IDictionary<Type, Argument> _args;

        public ArgumentContainer(params Argument[] arguments)
        {
            _args = new Dictionary<Type, Argument>();

            foreach(var argument in arguments)
            {
                Add(argument);
            }
        }

        public Argument this[Type key] { get => _args[key]; set => _args[key] = value; }

        public bool TryGetArgument<T>(out T output)
            where T : Argument
        {
            if (_args.TryGetValue(typeof(T), out var arg))
            {
                output = (T) arg;
                return true;
            }

            output = default;
            return false;
        }

        public ICollection<Type> Keys => _args.Keys;

        public ICollection<Argument> Values => _args.Values;

        public int Count => _args.Count;

        public bool IsReadOnly => _args.IsReadOnly;

        /// <summary>
        /// This method is not supported, left for <see cref="{IDictionary<Type, Argument>}"/> interface support
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        [Obsolete]
        public void Add(Type key, Argument value)
        {
            throw new InvalidOperationException("Not supported operation");
        }

        /// <summary>
        /// This method is not supported, left for <see cref="{IDictionary<Type, Argument>}"/> interface support
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        [Obsolete]
        public void Add(KeyValuePair<Type, Argument> item)
        {
            Add(item.Value);
        }

        /// <summary>
        /// Clears collection of arguments
        /// </summary>
        public void Clear()
        {
            _args.Clear();
        }

        /// <summary>
        /// Returns if contains item
        /// </summary>
        /// <param name="item">Searching item</param>
        /// <returns>Returns if contains item</returns>
        public bool Contains(KeyValuePair<Type, Argument> item)
        {
            return _args.Contains(item);
        }

        /// <summary>
        /// Adds argument to collection
        /// </summary>
        /// <param name="value">Argument that should be added to collection</param>
        public void Add(params Argument[] values)
        {
            foreach(var value in values) 
            {
                _args.Add(value.GetType(), value);
            }
        }

        /// <summary>
        /// Checks if container contains output and input parameters
        /// </summary>
        /// <returns></returns>
        public bool ContainsInputOutput()
        {
            return ((ContainsKey(typeof(InputArgument)) && !ContainsKey(typeof(ConcatArgument))) ||
                    (!ContainsKey(typeof(InputArgument)) && ContainsKey(typeof(ConcatArgument))))
                    && ContainsKey(typeof(OutputArgument));
        }

        /// <summary>
        /// Checks if contains argument of type
        /// </summary>
        /// <param name="key">Type of argument is seraching</param>
        /// <returns></returns>
        public bool ContainsKey(Type key)
        {
            return _args.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<Type, Argument>[] array, int arrayIndex)
        {
            _args.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<Type, Argument>> GetEnumerator()
        {
            return _args.GetEnumerator();
        }

        public bool Remove(Type key)
        {
            return _args.Remove(key);
        }

        public bool Remove(KeyValuePair<Type, Argument> item)
        {
            return _args.Remove(item);
        }

        public bool TryGetValue(Type key, out Argument value)
        {
            return _args.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _args.GetEnumerator();
        }

        /// <summary>
        /// Shortcut for finding arguments inside collection
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <returns></returns>
        public T Find<T>() where T : Argument
        {
            if (ContainsKey(typeof(T)))
                return (T)_args[typeof(T)];
            return null;
        }
        /// <summary>
        /// Shortcut for checking if contains arguments inside collection
        /// </summary>
        /// <typeparam name="T">Type of argument</typeparam>
        /// <returns></returns>
        public bool Contains<T>() where T : Argument
        {
            if (ContainsKey(typeof(T)))
                return true;
            return false;
        }
    }
}
