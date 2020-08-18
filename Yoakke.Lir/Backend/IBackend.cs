namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// Interface for every IR backend, that compiles the IR to some other representation.
    /// </summary>
    public interface IBackend
    {
        /// <summary>
        /// The toolchain used by this backend.
        /// </summary>
        public Toolchain Toolchain { get; set; }

        /// <summary>
        /// Checks, if the given <see cref="TargetTriplet"/> is supported by this backend.
        /// </summary>
        /// <param name="targetTriplet"></param>
        /// <returns></returns>
        public bool IsSupported(TargetTriplet targetTriplet);

        /// <summary>
        /// Compiles the given <see cref="Assembly"/> to the backend's representation.
        /// </summary>
        /// <param name="targetTriplet">The target to compile to.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to compile.</param>
        /// <returns>The backend's code representation.</returns>
        public string Compile(TargetTriplet targetTriplet, Assembly assembly);
    }
}
