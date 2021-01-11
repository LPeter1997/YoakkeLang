namespace Yoakke.Lir.Passes
{
    /// <summary>
    /// An iterface for IR code transformations and passes.
    /// </summary>
    public interface ICodePass
    {
        /// <summary>
        /// True, if this pass only makes sense to be ran once.
        /// </summary>
        public bool IsSinglePass { get; }

        /// <summary>
        /// Does a pass on the given <see cref="UncheckedAssembly"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="UncheckedAssembly"/> to do the pass on.</param>
        /// <param name="changed">True is output, if the code was changed in any way.</param>
        public void Pass(UncheckedAssembly assembly, out bool changed);
    }
}
