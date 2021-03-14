using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Debugger : IDebugger
    {
        // Threading, disposition state
        private bool disposed;
        private volatile bool running;
        private Thread loopThread;
        // Communication between threads
        private BlockingCollection<Action> queuedActions;
        // Bookkeeping
        private Dictionary<UInt32, TaskCompletionSource<IProcess>> unreturnedProcesses;
        private Dictionary<UInt32, Win32Process> processes;

        public Win32Debugger()
        {
            disposed = false;
            running = false;
            loopThread = new Thread(RunLoop);

            queuedActions = new BlockingCollection<Action>();

            unreturnedProcesses = new Dictionary<uint, TaskCompletionSource<IProcess>>();
            processes = new Dictionary<UInt32, Win32Process>();

            // NOTE: Start the thread last so everything will be initialized
            loopThread.Start();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            running = false;
            loopThread.Join();
        }

        public IProcess StartProcess(string path, string? args = null)
        {
            var tcs = new TaskCompletionSource<IProcess>();
            queuedActions.Add(() =>
            {
                try
                {
                    var process = WinApi.CreateDebuggableProcess(path, args);
                    unreturnedProcesses.Add(process.Id, tcs);
                    processes.Add(process.Id, process);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task.Result;
        }

        public void ContinueProcess(IProcess process)
        {
            queuedActions.Add(() => ((Win32Process)process).ContinueExecution());
        }

        public void StepProcess(IProcess process)
        {
            queuedActions.Add(() => ((Win32Process)process).StepInstruction());
        }

        private void CreateProcessDebugEvent(Win32Process process, ref WinApi.CREATE_PROCESS_DEBUG_INFO info)
        {
            Console.WriteLine("create process");
            // NOTE: We assign the result here as this is when we know the start address of the process
            if (unreturnedProcesses.Remove(process.Id, out var tcs))
            {
                // Create the main thread, connect it up with the process
                var mainThread = WinApi.MakeThreadObject(info.hThread);
                process.AddThread(mainThread);
                // Finally push the result back
                tcs.SetResult(process);
            }
            else
            {
                // Something went very wrong
                throw new InvalidOperationException();
            }
        }

        private void ExitProcessDebugEvent(Win32Process process, ref WinApi.EXIT_PROCESS_DEBUG_INFO info)
        {
            Console.WriteLine($"exit process (code {info.dwExitCode})");
            // Just remove from bookkeeping
            processes.Remove(process.Id);
        }

        private void RunLoop()
        {
            running = true;
            var debugEvent = new WinApi.DEBUG_EVENT();
            while (running)
            {
                // First check if there's something to perform and perform them
                for (; queuedActions.TryTake(out var action); action()) ;
                // Now handle debug events
                for (; WinApi.TryGetDebugEvent(out debugEvent); ProcessDebugEvent(ref debugEvent)) ;
                // Finished everything, ease the thread a bit
                Thread.Sleep(0);
            }
        }

        private void ProcessDebugEvent(ref WinApi.DEBUG_EVENT debugEvent)
        {
            var process = processes[debugEvent.dwProcessId];
            switch (debugEvent.dwDebugEventCode)
            {
            // Handle process modifier events here
            case WinApi.CREATE_PROCESS_DEBUG_EVENT:
            {
                CreateProcessDebugEvent(process, ref debugEvent.u.CreateProcessInfo);
                var thread = process.GetThread(debugEvent.dwThreadId);
                WinApi.ContinueDebugThread(thread, true);
            } break;

            case WinApi.EXIT_PROCESS_DEBUG_EVENT:
            {
                ExitProcessDebugEvent(process, ref debugEvent.u.ExitProcess);
                var thread = process.GetThread(debugEvent.dwThreadId);
                WinApi.ContinueDebugThread(thread, true);
            } break;

            // Anything else we do in the process
            default:
                process.HandleDebugEvent(ref debugEvent);
                break;
            }
        }
    }
}
