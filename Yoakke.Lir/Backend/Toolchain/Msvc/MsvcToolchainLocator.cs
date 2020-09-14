using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// An <see cref="IToolchainLocator"/> for the MSVC toolchain.
    /// </summary>
    public class MsvcToolchainLocator : IToolchainLocator
    {
        public IEnumerable<IToolchain> Locate()
        {
            // First we need to call vswhere, as it can tell us Visual C++ tools installation
            // We construct the expected path for VsWhere
            var vsWherePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");
            // If there's no vswhere, there's probably no valid installation
            if (!File.Exists(vsWherePath)) yield break;
            // There is vswhere, call it and get the list of results
            var installations = ExecuteVsWhere(vsWherePath);
            // If no match, return
            if (installations == null) yield break;
            // Go through each installation, pick the first one that finds the assembler and linker
            foreach (var installation in installations.RootElement.EnumerateArray())
            {
                var installationPath = installation.GetProperty("installationPath").ToString();
                var installationVersion = installation.GetProperty("displayName").ToString();
                Debug.Assert(installationPath != null);
                Debug.Assert(installationVersion != null);
                installationVersion = installationVersion.ToLower().Replace(' ', '-');
                // Get vcvarsall
                var vcvarsallPath = Path.Combine(installationPath, "VC", "Auxiliary", "Build", "vcvarsall.bat");
                if (!File.Exists(vcvarsallPath)) continue;
                // We have vcvarsall we assume everything else is present
                yield return new MsvcToolchain(installationVersion, vcvarsallPath);
            }
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
