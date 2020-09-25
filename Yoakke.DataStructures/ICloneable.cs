using System;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// An interface for deep-copyable objects.
    /// </summary>
    /// <typeparam name="T">The type of the resulting clone.</typeparam>
    public interface ICloneable<T> : ICloneable
    {
        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>The cloned object.</returns>
        new public T Clone();
    }
}
