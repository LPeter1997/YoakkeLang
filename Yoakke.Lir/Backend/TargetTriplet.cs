using System;
using System.Reflection;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// The target-triplet defining the target machine.
    /// </summary>
    public readonly struct TargetTriplet
    {
        /// <summary>
        /// Gets the <see cref="TargetTriplet"/> representing this machine.
        /// </summary>
        public TargetTriplet HostMachine => GetHostMachine();

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

        private static TargetTriplet GetHostMachine()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            // TODO: Use RuntimeInformation.ProcessArchitecture instead
            var cpuFamily = asm.GetName().ProcessorArchitecture switch
            {
                ProcessorArchitecture.X86 => CpuFamily.X86,
                _ => throw new NotImplementedException(),
            };
            var os = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => OperatingSystem.Windows,
                _ => throw new NotImplementedException(),
            };
            return new TargetTriplet(cpuFamily, os);
        }
    }
}
