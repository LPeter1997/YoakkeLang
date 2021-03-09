using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Debugger : IDebugger
    {
        public event IDebugger.ProcessStartedEventHandler? ProcessStartedEvent;
        public event IDebugger.ProcessTerminatedEventHandler? ProcessTerminatedEvent;

        public IProcess StartProcess(string path, string? commandLine)
        {
            unsafe
            {
                fixed (char* commandLineCstr = commandLine)
                {
                    var startupInfo = new STARTUPINFOW();
                    startupInfo.cb = (uint)sizeof(STARTUPINFOW);
                    var processInfo = new PROCESS_INFORMATION();
                    var creationResult = PInvoke.CreateProcess(
                        path,
                        commandLineCstr,
                        null, null,
                        false,
                        PROCESS_CREATION_FLAGS.DEBUG_ONLY_THIS_PROCESS,
                        null,
                        null,
                        in startupInfo,
                        out processInfo);
                    if (!creationResult)
                    {
                        // Failed to create the process
                        // TODO: Proper exception
                        throw new NotImplementedException();
                    }
                    return new Win32Process(processInfo.hProcess);
                }
            }
        }
    }
}
