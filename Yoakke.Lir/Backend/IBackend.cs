using System.IO;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// Interface for every IR backend, that compiles the IR to some other representation.
    /// </summary>
    public interface IBackend
    {
        /// <summary>
        /// The <see cref="TargetTriplet"/> of the backend.
        /// </summary>
        public TargetTriplet TargetTriplet { get; set; }

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
