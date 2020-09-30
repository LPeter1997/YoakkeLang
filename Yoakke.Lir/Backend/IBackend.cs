namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// Interface for every IR backend, that compiles the IR to some other representation.
    /// </summary>
    public interface IBackend
    {
        /// <summary>
        /// Checks, if the given <see cref="TargetTriplet"/> is supported by this backend.
        /// </summary>
        /// <param name="targetTriplet">The <see cref="TargetTriplet"/> to check support for.</param>
        /// <returns>True, if the <see cref="TargetTriplet"/> is supported.</returns>
        public bool IsSupported(TargetTriplet targetTriplet);

        /// <summary>
        /// Compiles the given <see cref="Build"/>s <see cref="Assembly"/> to the backend's representation.
        /// </summary>
        /// <param name="build">The <see cref="Build"/> to work on.</param>
        public void Compile(Build build);
    }
}
