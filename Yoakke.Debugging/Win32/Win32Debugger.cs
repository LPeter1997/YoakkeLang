using System;
using System.Collections.Concurrent;
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
        
        private ConcurrentQueue<(string Path, string? CommandLine)> processesToAdd = new ConcurrentQueue<(string Path, string? CommandLine)>();

        private Dictionary<uint, Win32Process> processes = new Dictionary<uint, Win32Process>();
        private Dictionary<uint, Win32Thread> threads = new Dictionary<uint, Win32Thread>();

        public event IDebugger.ProcessStartedEventHandler? ProcessStarted;
        public event IDebugger.ProcessTerminatedEventHandler? ProcessTerminated;
        public event IDebugger.ThreadStartedEventHandler? ThreadStarted;
        public event IDebugger.ThreadTerminatedEventHandler? ThreadTerminated;

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

        public void StartProcess(string path, string? commandLine) =>
            processesToAdd.Enqueue((path, commandLine));

        private void RunMessageLoop()
        {
            running = true;
            var debugEvent = new DEBUG_EVENT();
            while (running)
            {
                if (processesToAdd.TryDequeue(out var toAdd))
                {
                    StartProcessInternal(toAdd.Path, toAdd.CommandLine);
                }

                if (PInvoke.WaitForDebugEventEx(out debugEvent, 0))
                {
                    ProcessEvent(ref debugEvent);
                    PInvoke.ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, 0x00010002);
                }
                else
                {
                    var err = Marshal.GetLastWin32Error();
                    if (err == Constants.ERROR_SEM_TIMEOUT)
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
            switch (debugEvent.dwDebugEventCode)
            {
            case 3: // CREATE_PROCESS_DEBUG_EVENT
            {
                var info = debugEvent.u.CreateProcessInfo;
                var pid = PInvoke.GetProcessId(info.hProcess);
                var process = new Win32Process(info.hProcess);
                processes.Add(pid, process);
                ProcessStarted?.Invoke(this, new ProcessStartedEventArgs { Process = process });
            } break;

            case 5: // EXIT_PROCESS_DEBUG_EVENT
            {
                var info = debugEvent.u.ExitProcess;
                var process = processes[debugEvent.dwProcessId];
                var exitCode = (int)info.dwExitCode;
                processes.Remove(debugEvent.dwProcessId);
                ProcessTerminated?.Invoke(this, new ProcessTerminatedEventArgs { Process = process, ExitCode = exitCode });
            } break;

            case 2: // CREATE_THREAD_DEBUG_EVENT
            {
                var info = debugEvent.u.CreateThread;
                var tid = PInvoke.GetThreadId(info.hThread);
                var thread = new Win32Thread(processes[debugEvent.dwProcessId], info.hThread);
                threads.Add(tid, thread);
                ThreadStarted?.Invoke(this, new ThreadStartedEventArgs { Thread = thread });
            } break;

            case 4: // EXIT_THREAD_DEBUG_EVENT
            {
                var info = debugEvent.u.ExitThread;
                var thread = threads[debugEvent.dwThreadId];
                var exitCode = (int)info.dwExitCode;
                threads.Remove(debugEvent.dwThreadId);
                ThreadTerminated?.Invoke(this, new ThreadTerminatedEventArgs { Thread = thread, ExitCode = exitCode });
            } break;

            default:
                // TODO
                Console.WriteLine($"TODO Unhandled event {debugEvent.dwDebugEventCode}");
                break;
            }
        }

        private void StartProcessInternal(string path, string? commandLine)
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
                }
            }
        }
    }
}
