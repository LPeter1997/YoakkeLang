using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            foreach (var (vsPath, vsVersion) in LocateVsInstallations())
            {
                if (!LocateMsvcSdk(vsPath, out var msvcSdkDir)) continue;
                if (!LocateWindowsSDK(vsPath, out var windowsSdkDir, out var windowsSdkVer)) continue;

                yield return new MsvcToolchain(vsVersion, msvcSdkDir, windowsSdkDir, windowsSdkVer);
            }
        }

        // (path, version)
        private IEnumerable<(string, string)> LocateVsInstallations()
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

                yield return (installationPath, installationVersion);
            }
        }

        private bool LocateMsvcSdk(string vsPath, out string msvcSdkPath)
        {
            msvcSdkPath = string.Empty;
            var msvcSdksRoot = Path.Combine(vsPath, "VC", "Tools", "Msvc");
            var subdirs = Directory.EnumerateDirectories(msvcSdksRoot);
            foreach (var subdir in subdirs)
            {
                // NOTE: We should have a strategy or check what to choose
                // For now we just blindly choose the first subdirectory
                msvcSdkPath = subdir;
                return true;
            }
            return false;
        }

        private bool LocateWindowsSDK(string vsPath, out string sdkDir, out string sdkVer)
        {
            sdkDir = string.Empty;
            sdkVer = string.Empty;
            // First we craft a batch command, that execures winsdk, and returns the SDK path and version
            var winsdkPath = Path.Combine(vsPath, "Common7", "Tools", "vsdevcmd", "core", "winsdk.bat");
            var script = @$"@ECHO off
call ""{winsdkPath}"" >nul 2>&1
echo %WindowsSdkDir%
echo %WindowsSDKLibVersion%";
            // Write this batch to file, call it, extract the two lines echoed back
            string batchFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".bat");
            File.WriteAllText(batchFileName, script);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = batchFileName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            var outputBuilder = new StringBuilder();
            proc.OutputDataReceived += (_, e) => outputBuilder.AppendLine(e.Data);
            proc.ErrorDataReceived += (_, e) => outputBuilder.AppendLine(e.Data);
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
            // Remove the file
            File.Delete(batchFileName);
            // Extract the two lines
            var lines = outputBuilder.ToString().Split(Environment.NewLine);
            if (lines.Length < 2) return false;
            sdkDir = lines[0].Trim();
            sdkVer = lines[1].Trim();
            if (!Directory.Exists(Path.Combine(sdkDir, "Lib", sdkVer))) return false;
            return true;
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
