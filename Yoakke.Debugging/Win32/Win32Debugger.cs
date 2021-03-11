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
        private ConcurrentDictionary<UInt32, Win32Process> processes;

        public Win32Debugger()
        {
            disposed = false;
            running = false;
            loopThread = new Thread(RunLoop);

            queuedActions = new BlockingCollection<Action>();
            processes = new ConcurrentDictionary<uint, Win32Process>();
            
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
                    processes.TryAdd(process.Id, process);
                    tcs.SetResult(process);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task.Result;
        }

        private void CreateProcessDebugEvent(Win32Process process, ref WinApi.CREATE_PROCESS_DEBUG_INFO info)
        {
            Console.WriteLine("create process");
        }

        private void CreateThreadDebugEvent(Win32Process process, ref WinApi.CREATE_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine("create thread");
        }

        private void ExceptionDebugEvent(Win32Process process, ref WinApi.EXCEPTION_DEBUG_INFO info)
        {
            Console.WriteLine("exception");
        }

        private void ExitProcessDebugEvent(Win32Process process, ref WinApi.EXIT_PROCESS_DEBUG_INFO info)
        {
            Console.WriteLine("exit process");
        }

        private void ExitThreadDebugEvent(Win32Process process, ref WinApi.EXIT_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine("exit thread");
        }

        private void LoadDllDebugEvent(Win32Process process, ref WinApi.LOAD_DLL_DEBUG_INFO info)
        {
            Console.WriteLine("load dll");
        }

        private void OutputStringDebugEvent(Win32Process process, ref WinApi.OUTPUT_DEBUG_STRING_INFO info)
        {
            Console.WriteLine("output string");
        }

        private void RipDebugEvent(Win32Process process, ref WinApi.RIP_INFO info)
        {
            Console.WriteLine("RIP");
        }

        private void UnloadDllDebugEvent(Win32Process process, ref WinApi.UNLOAD_DLL_DEBUG_INFO info)
        {
            Console.WriteLine("unload dll");
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
                while (WinApi.TryGetDebugEvent(out debugEvent))
                {
                    ProcessDebugEvent(ref debugEvent);
                    WinApi.ContinueDebugEvent(ref debugEvent);
                }
                Thread.Sleep(0);
            }
        }

        private void ProcessDebugEvent(ref WinApi.DEBUG_EVENT debugEvent)
        {
            var process = processes[debugEvent.dwProcessId];
            switch (debugEvent.dwDebugEventCode)
            {
            case WinApi.CREATE_PROCESS_DEBUG_EVENT:
                CreateProcessDebugEvent(process, ref debugEvent.u.CreateProcessInfo);
                break;
            case WinApi.CREATE_THREAD_DEBUG_EVENT:
                CreateThreadDebugEvent(process, ref debugEvent.u.CreateThread);
                break;
            case WinApi.EXCEPTION_DEBUG_EVENT:
                ExceptionDebugEvent(process, ref debugEvent.u.Exception);
                break;
            case WinApi.EXIT_PROCESS_DEBUG_EVENT:
                ExitProcessDebugEvent(process, ref debugEvent.u.ExitProcess);
                break;
            case WinApi.EXIT_THREAD_DEBUG_EVENT:
                ExitThreadDebugEvent(process, ref debugEvent.u.ExitThread);
                break;
            case WinApi.LOAD_DLL_DEBUG_EVENT:
                LoadDllDebugEvent(process, ref debugEvent.u.LoadDll);
                break;
            case WinApi.OUTPUT_DEBUG_STRING_EVENT:
                OutputStringDebugEvent(process, ref debugEvent.u.DebugString);
                break;
            case WinApi.RIP_EVENT:
                RipDebugEvent(process, ref debugEvent.u.RipInfo);
                break;
            case WinApi.UNLOAD_DLL_DEBUG_EVENT:
                UnloadDllDebugEvent(process, ref debugEvent.u.UnloadDll);
                break;

            default: throw new NotImplementedException();
            }
        }
    }
}
