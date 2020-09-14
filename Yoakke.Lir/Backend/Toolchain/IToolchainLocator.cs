using System.Collections.Generic;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Locates a given supported <see cref="IToolchain"/>.
    /// </summary>
    public interface IToolchainLocator
    {
        /// <summary>
        /// Locates all the <see cref="IToolchain"/>s that this locator can.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> over all located <see cref="IToolchain"/>s.</returns>
        public IEnumerable<IToolchain> Locate();
    }
}
