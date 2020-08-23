using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// Base class for MSVC tools that require the environment set up by vcvarsall.
    /// </summary>
    public class MsvcToolBase
    {
        private string vcVarsAllPath;

        /// <summary>
        /// Initializes a new <see cref="MsvcToolBase"/>.
        /// </summary>
        /// <param name="vcVarsAllPath">The path to the 'vcvarsall' tool.</param>
        public MsvcToolBase(string vcVarsAllPath)
        {
            this.vcVarsAllPath = vcVarsAllPath;
        }

        /// <summary>
        /// Creates a <see cref="Process"/> that will have automatically have it's environment set up
        /// by 'vcvarsall'.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="targetTriplet">The <see cref="TargetTriplet"/> the tool should be set up for.</param>
        /// <returns>The <see cref="Process"/> that can be started to execute the command.</returns>
        protected Process InvokeWithEnvironment(string command, TargetTriplet targetTriplet)
        {
            var archId = GetArchId(targetTriplet);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C (\"{vcVarsAllPath}\" {archId}) && ({command})",
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            return proc;
        }

        private string GetArchId(TargetTriplet targetTriplet) => targetTriplet.CpuFamily switch
        {
            CpuFamily.X86 => "x86",
            _ => throw new NotImplementedException(),
        };
    }
}
