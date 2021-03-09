using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;
using Yoakke.Debugging.Events;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Debugger : IDebugger
    {
        private Thread thread;
        private volatile bool running;
        private Dictionary<uint, Win32Process> processes = new Dictionary<uint, Win32Process>();

        public event IDebugger.ProcessStartedEventHandler? ProcessStartedEvent;
        public event IDebugger.ProcessTerminatedEventHandler? ProcessTerminatedEvent;

        public Win32Debugger()
        {
            running = false;
            thread = new Thread(RunMessageLoop);
            thread.Start();
        }

        public void Dispose()
        {
            running = false;
            thread.Join();
            GC.SuppressFinalize(this);
        }

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
                    var process = new Win32Process(processInfo.hProcess);
                    processes.Add(processInfo.dwProcessId, process);
                    return process;
                }
            }
        }

        private void RunMessageLoop()
        {
            running = true;
            var debugEvent = new DEBUG_EVENT();
            while (running)
            {
                if (PInvoke.WaitForDebugEventEx(out debugEvent, 0))
                {
                    ProcessEvent(ref debugEvent);
                    PInvoke.ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, 0x00010002);
                }
                else
                {
                    var err = Marshal.GetLastWin32Error();
                    if (   err == Constants.ERROR_SEM_TIMEOUT 
                        || err == Constants.ERROR_INVALID_HANDLE)
                    {
                        // There was no event to process
                        Thread.Sleep(0);
                        continue;
                    }
                    // TODO: Proper error
                    throw new NotImplementedException($"Error {err}");
                }
            }
        }

        private void ProcessEvent(ref DEBUG_EVENT debugEvent)
        {
            var process = processes[debugEvent.dwProcessId];
            switch (debugEvent.dwDebugEventCode)
            {
            case 3: // CREATE_PROCESS_DEBUG_EVENT
            {
                ProcessStartedEvent?.Invoke(this, new ProcessStartedEventArgs { Process = process });
            } break;

            case 5: // EXIT_PROCESS_DEBUG_EVENT
            {
                var info = debugEvent.u.ExitProcess;
                ProcessTerminatedEvent?.Invoke(this, new ProcessTerminatedEventArgs { Process = process, ExitCode = (int)info.dwExitCode });
            } break;

            default:
                // TODO
                Console.WriteLine($"TODO Unhandled event {debugEvent.dwDebugEventCode}");
                break;
            }
        }
    }
}
