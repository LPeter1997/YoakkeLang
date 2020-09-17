using System.IO;

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
        /// Compiles the given <see cref="Assembly"/> to the backend's representation.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile.</param>
        /// <returns>The string representation of the backend.</returns>
        public string Compile(Assembly assembly);

        /// <summary>
        /// Compiles the given <see cref="Assembly"/> to the backend's representation.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile.</param>
        /// <param name="outputPath">The path of the output file.</param>
        public void Compile(Assembly assembly, string outputPath) =>
            File.WriteAllText(outputPath, Compile(assembly));
    }
}
