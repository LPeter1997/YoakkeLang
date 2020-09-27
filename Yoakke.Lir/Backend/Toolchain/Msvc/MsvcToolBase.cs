using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Yoakke.Lir.Status;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// Base class for MSVC tools that require the environment set up by vcvarsall.
    /// </summary>
    public abstract class MsvcToolBase : ITool
    {
        public string Version { get; }
        protected string MsvcSdk { get; }
        protected string WindowsSdk { get; }
        protected string WindowsSdkVersion { get; }

        /// <summary>
        /// Initializes a new <see cref="MsvcToolBase"/>.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="msvcSdk">The path to the MSVC SDK.</param>
        /// <param name="windowsSdk">The path to the Windows SDK.</param>
        /// <param name="windowsSdkVer">The Windows SDK version.</param>
        public MsvcToolBase(string version, string msvcSdk, string windowsSdk, string windowsSdkVer)
        {
            Version = version;
            MsvcSdk = msvcSdk;
            WindowsSdk = windowsSdk;
            WindowsSdkVersion = windowsSdkVer;
        }

        protected void InvokeWithEnvironment(string toolName, string arguments, Build build)
        {
            var archId = GetTargetMachineId(build.TargetTriplet);
            var toolPath = Path.Combine(MsvcSdk, "bin", "Hostx86", archId, toolName);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = toolPath,
                    Arguments = arguments,
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
            if (proc.ExitCode != 0)
            {
                var fullCommand = $"\"{toolPath}\" {arguments}";
                build.Report(new ToolchainError(this, fullCommand, outputBuilder.ToString()));
            }
        }

        protected static string GetTargetMachineId(TargetTriplet targetTriplet) => targetTriplet.CpuFamily switch
        {
            CpuFamily.X86 => "x86",
            _ => throw new NotSupportedException($"The CPU {targetTriplet.CpuFamily} is not supported by MSVC tools!"),
        };

        public bool IsSupported(TargetTriplet targetTriplet) =>
               targetTriplet.CpuFamily == CpuFamily.X86 
            && targetTriplet.OperatingSystem == OperatingSystem.Windows;

        public abstract void Execute(Build build);
    }
}
