using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A simple, non-thread-safe lazily initialized value.
    /// </summary>
    /// <typeparam name="T">The type of the lazily initialized value.</typeparam>
    public class Lazy<T> where T : notnull
    {
        /// <summary>
        /// True, if the value is already initialized.
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// The underlying, lazily initialized value.
        /// </summary>
        public T Value
        {
            get
            {
                if (!IsInitialized) value = initializer();
                IsInitialized = true;
                Debug.Assert(value != null);
                return value;
            }
        }

        private T? value;
        private Func<T> initializer;

        /// <summary>
        /// Initializes a new <see cref="Lazy{T}"/>.
        /// </summary>
        /// <param name="initializer">The initializer <see cref="Func{T}"/></param>.
        public Lazy(Func<T> initializer)
        {
            this.initializer = initializer;
        }

        public override bool Equals(object? obj) => Value.Equals(obj);
        public override int GetHashCode() => Value.GetHashCode();

        public override string? ToString() => Value.ToString();
    }
}
