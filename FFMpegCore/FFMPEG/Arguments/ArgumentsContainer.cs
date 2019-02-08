using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Container of arguments represented parameters of FFMPEG process
    /// </summary>
    public class ArgumentsContainer : IDictionary<Type, Argument>
    {
        Dictionary<Type, Argument> _args;

        public ArgumentsContainer()
        {
            _args = new Dictionary<Type, Argument>();
        }

        public Argument this[Type key] { get => ((IDictionary<Type, Argument>)_args)[key]; set => ((IDictionary<Type, Argument>)_args)[key] = value; }

        public ICollection<Type> Keys => ((IDictionary<Type, Argument>)_args).Keys;

        public ICollection<Argument> Values => ((IDictionary<Type, Argument>)_args).Values;

        public int Count => ((IDictionary<Type, Argument>)_args).Count;

        public bool IsReadOnly => ((IDictionary<Type, Argument>)_args).IsReadOnly;

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
            throw new InvalidOperationException("Not supported operation");
        }

        /// <summary>
        /// Clears collection of arguments
        /// </summary>
        public void Clear()
        {
            ((IDictionary<Type, Argument>)_args).Clear();
        }

        /// <summary>
        /// Returns if contains item
        /// </summary>
        /// <param name="item">Searching item</param>
        /// <returns>Returns if contains item</returns>
        public bool Contains(KeyValuePair<Type, Argument> item)
        {
            return ((IDictionary<Type, Argument>)_args).Contains(item);
        }

        /// <summary>
        /// Adds argument to collection
        /// </summary>
        /// <param name="value">Argument that should be added to collection</param>
        public void Add(Argument value)
        {
            ((IDictionary<Type, Argument>)_args).Add(value.GetType(), value);
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
            return ((IDictionary<Type, Argument>)_args).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<Type, Argument>[] array, int arrayIndex)
        {
            ((IDictionary<Type, Argument>)_args).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<Type, Argument>> GetEnumerator()
        {
            return ((IDictionary<Type, Argument>)_args).GetEnumerator();
        }

        public bool Remove(Type key)
        {
            return ((IDictionary<Type, Argument>)_args).Remove(key);
        }

        public bool Remove(KeyValuePair<Type, Argument> item)
        {
            return ((IDictionary<Type, Argument>)_args).Remove(item);
        }

        public bool TryGetValue(Type key, out Argument value)
        {
            return ((IDictionary<Type, Argument>)_args).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<Type, Argument>)_args).GetEnumerator();
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
