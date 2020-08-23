using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// An <see cref="IToolchainLocator"/> for the MSVC toolchain.
    /// </summary>
    public class MsvcToolchainLocator : IToolchainLocator
    {
        public bool TryLocate(out IToolchain? toolchain)
        {
            toolchain = null;
            // First we need to call vswhere, as it can tell us Visual C++ tools installation
            // We construct the expected path for VsWhere
            var vsWherePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");
            // If there's no vswhere, there's probably no valid installation
            if (!File.Exists(vsWherePath)) return false;
            // There is vswhere, call it and get the list of results
            var installations = ExecuteVsWhere(vsWherePath);
            // If no match, return
            if (installations == null) return false;
            // Go through each installation, pick the first one that finds the assembler and linker
            foreach (var installation in installations.RootElement.EnumerateArray())
            {
                var installationPath = installation.GetProperty("installationPath").ToString();
                Debug.Assert(installationPath != null);
                // Get vcvarsall
                var vcvarsallPath = Path.Combine(installationPath, "VC", "Auxiliary", "Build", "vcvarsall.bat");
                if (!File.Exists(vcvarsallPath)) continue;
                // We have vcvarsall we assume everything else is present
                toolchain = new MsvcToolchain(vcvarsallPath);
                return true;
            }
            // No matching installation
            return false;
        }

        public bool TryLocateAssembler(out IAssembler? assembler)
        {
            if (TryLocate(out var tc))
            {
                assembler = (tc as MsvcToolchain)?.Assembler;
                return true;
            }
            assembler = null;
            return false;
        }

        public bool TryLocateLinker(out ILinker? linker)
        {
            if (TryLocate(out var tc))
            {
                linker = (tc as MsvcToolchain)?.Linker;
                return true;
            }
            linker = null;
            return false;
        }

        public bool TryLocateArchiver(out IArchiver? archiver)
        {
            if (TryLocate(out var tc))
            {
                archiver = (tc as MsvcToolchain)?.Archiver;
                return true;
            }
            archiver = null;
            return false;
        }

        private static JsonDocument? ExecuteVsWhere(string path)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    // All products (Community, Professional, Enterprise), contains VC++ tools, output as JSON
                    Arguments = "-products * -requires Microsoft.VisualStudio.Workload.VCTools -format json",
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode != 0) return null;
            return JsonDocument.Parse(output);
        }
    }
}
