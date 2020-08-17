using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// The target-triplet defining the target machine.
    /// </summary>
    public readonly struct TargetTriplet
    {
        /// <summary>
        /// The <see cref="CpuFamily"/> of the <see cref="TargetTriplet"/>.
        /// </summary>
        public readonly CpuFamily CpuFamily;
        /// <summary>
        /// The <see cref="Vendor"/> of the <see cref="TargetTriplet"/>.
        /// </summary>
        public readonly Vendor Vendor;
        /// <summary>
        /// The <see cref="OperatingSystem"/> of the <see cref="TargetTriplet"/>.
        /// </summary>
        public readonly OperatingSystem OperatingSystem;

        /// <summary>
        /// Initializes a new <see cref="TargetTriplet"/>.
        /// </summary>
        /// <param name="cpuFamily">The <see cref="CpuFamily"/> of the target triplet.</param>
        /// <param name="vendor">The <see cref="Vendor"/> of the target triplet.</param>
        /// <param name="operatingSystem">The <see cref="OperatingSystem"/> of the target triplet.</param>
        public TargetTriplet(CpuFamily cpuFamily, Vendor vendor, OperatingSystem operatingSystem)
        {
            CpuFamily = cpuFamily;
            Vendor = vendor;
            OperatingSystem = operatingSystem;
        }

        /// <summary>
        /// Initializes a new <see cref="TargetTriplet"/> with <see cref="Vendor.Unknown"/>.
        /// </summary>
        /// <param name="cpuFamily">The <see cref="CpuFamily"/> of the target triplet.</param>
        /// <param name="operatingSystem">The <see cref="OperatingSystem"/> of the target triplet.</param>
        public TargetTriplet(CpuFamily cpuFamily, OperatingSystem operatingSystem)
            : this(cpuFamily, Vendor.Unknown, operatingSystem)
        {
        }

        public override string ToString() => $"{CpuFamily}-{Vendor}-{OperatingSystem}".ToLower();
    }
}
