using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Backend.Toolchain.Msvc;

namespace Yoakke.Lir.Backend.Toolchain
{
    /// <summary>
    /// Utilities for obtaining <see cref="IToolchain"/>s.
    /// </summary>
    public static class Toolchains
    {
        /// <summary>
        /// Finds all of the <see cref="IToolchain"/>s on the current platform.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{T}"/> of all of the <see cref="IToolchain"/>s on the platform.</returns>
        public static IEnumerable<IToolchain> All()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new MsvcToolchainLocator().Locate();
            }
            return Enumerable.Empty<IToolchain>();
        }

        /// <summary>
        /// Finds all of the <see cref="IToolchain"/>s on the current platform that supports a 
        /// specific <see cref="TargetTriplet"/>.
        /// Also defaults the <see cref="TargetTriplet"/> for the results to the checked one.
        /// </summary>
        /// <param name="targetTriplet">The <see cref="TargetTriplet"/> to check support for.</param>
        /// <returns>The<see cref="IEnumerable{T}"/> of all of the<see cref="IToolchain"/> s on the platform
        /// that supports the given <see cref="TargetTriplet"/>.</returns>
        public static IEnumerable<IToolchain> Supporting(TargetTriplet targetTriplet) =>
            All().Where(tc => tc.IsSupported(targetTriplet));
    }
}
