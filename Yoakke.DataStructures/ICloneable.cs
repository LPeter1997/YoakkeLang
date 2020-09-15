namespace Yoakke.DataStructures
{
    /// <summary>
    /// An interface for deep-copyable objects.
    /// </summary>
    /// <typeparam name="T">The type of the resulting clone.</typeparam>
    public interface ICloneable<T>
    {
        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>The cloned object.</returns>
        public T Clone();
    }
}
