using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// Base class for MSVC tools that require the environment set up by vcvarsall.
    /// </summary>
    public abstract class MsvcToolBase : ITool
    {
        private string vcVarsAllPath;

        public string Version { get; }

        /// <summary>
        /// Initializes a new <see cref="MsvcToolBase"/>.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="vcVarsAllPath">The path to the 'vcvarsall' tool.</param>
        public MsvcToolBase(string version, string vcVarsAllPath)
        {
            Version = version;
            this.vcVarsAllPath = vcVarsAllPath;
        }

        protected int InvokeWithEnvironment(string command, TargetTriplet targetTriplet)
        {
            var archId = GetTargetMachineId(targetTriplet);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C (\"{vcVarsAllPath}\" {archId}) && ({command})",
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
                Console.Error.WriteLine($"Command '{command}' failed to run!");
                Console.Error.Write(outputBuilder);
            }
            return proc.ExitCode;
        }

        protected static string GetTargetMachineId(TargetTriplet targetTriplet) => targetTriplet.CpuFamily switch
        {
            CpuFamily.X86 => "x86",
            _ => throw new NotSupportedException($"The CPU {targetTriplet.CpuFamily} is not supported by MSVC tools!"),
        };

        public bool IsSupported(TargetTriplet targetTriplet) =>
               targetTriplet.CpuFamily == CpuFamily.X86 
            && targetTriplet.OperatingSystem == OperatingSystem.Windows;

        public abstract int Execute(Build build);
    }
}
