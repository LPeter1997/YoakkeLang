namespace Yoakke.Compiler
{
    /// <summary>
    /// The types of output the compiler can produce.
    /// </summary>
    public enum OutputType
    {
        /// <summary>
        /// Produce IR code.
        /// </summary>
        IR,
        /// <summary>
        /// Produce object code.
        /// </summary>
        Obj,
        /// <summary>
        /// Produce an executable.
        /// </summary>
        Exe,
        /// <summary>
        /// Produce a shared library.
        /// </summary>
        Shared,
    }
}
