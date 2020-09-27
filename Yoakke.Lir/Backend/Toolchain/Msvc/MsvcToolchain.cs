using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yoakke.Lir.Backend.Backends;
using Yoakke.Lir.Status;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC toolchain.
    /// </summary>
    public class MsvcToolchain : StandardToolchain
    {
        public override IBackend Backend { get; } = new MasmX86Backend();
        public override IAssembler Assembler { get; }
        public override ILinker Linker { get; }
        public override IArchiver Archiver { get; }

        public override string Version { get; }

        public MsvcToolchain(string version, string msvcSdk, string windowsSdk, string windowsSdkVer)
        {
            Version = version;
            Assembler = new MsvcAssembler(version, msvcSdk, windowsSdk, windowsSdkVer);
            Linker = new MsvcLinker(version, msvcSdk, windowsSdk, windowsSdkVer);
            Archiver = new MsvcArchiver(version, msvcSdk, windowsSdk, windowsSdkVer);
        }

        public override string ToString() => $"msvc-{Version}";
    }
}
