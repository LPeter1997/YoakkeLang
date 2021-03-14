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
        private Dictionary<UInt32, Win32Thread> threads;

        public Win32Debugger()
        {
            disposed = false;
            running = false;
            loopThread = new Thread(RunLoop);

            queuedActions = new BlockingCollection<Action>();

            unreturnedProcesses = new Dictionary<uint, TaskCompletionSource<IProcess>>();
            processes = new Dictionary<UInt32, Win32Process>();
            threads = new Dictionary<UInt32, Win32Thread>();

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
                    unreturnedProcesses.TryAdd(process.Id, tcs);
                    processes.TryAdd(process.Id, process);
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
            // NOTE: We assign the result here as this is when we know the start address of the process
            if (unreturnedProcesses.Remove(process.Id, out var tcs))
            {
                // Set start address
                unsafe
                {
                    process.StartAddress = (nuint)info.lpStartAddress;
                }
                // Create the main thread, connect it up with the process
                var mainThread = WinApi.MakeThreadObject(process, info.hThread);
                process.MainThread = mainThread;
                threads.Add(mainThread.Id, mainThread);
                // Finally push the result back
                tcs.SetResult(process);
            }
            else
            {
                // Something went very wrong
                throw new InvalidOperationException();
            }
        }

        private void CreateThreadDebugEvent(Win32Process process, ref WinApi.CREATE_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine("create thread");
            var thread = WinApi.MakeThreadObject(process, info.hThread);
            threads.Add(thread.Id, thread);
        }

        private void ExceptionDebugEvent(Win32Process process, ref WinApi.EXCEPTION_DEBUG_INFO info)
        {
            Console.WriteLine($"IP = {WinApi.GetInstructionPointer((Win32Thread)process.MainThread)}");
            if (info.dwFirstChance != 0)
            {
                // First time encounter
                switch (info.ExceptionRecord.ExceptionCode)
                {
                case WinApi.EXCEPTION_ACCESS_VIOLATION:
                    Console.WriteLine("Exception: ACCESS VIOLATION");
                    break;
                case WinApi.EXCEPTION_ARRAY_BOUNDS_EXCEEDED:
                    Console.WriteLine("Exception: ARRAY BOUNDS EXCEEDED");
                    break;
                case WinApi.EXCEPTION_BREAKPOINT:
                    Console.WriteLine("Exception: BREAKPOINT");
                    break;
                case WinApi.EXCEPTION_DATATYPE_MISALIGNMENT:
                    Console.WriteLine("Exception: DATATYPE MISALIGNMENT");
                    break;
                case WinApi.EXCEPTION_FLT_DENORMAL_OPERAND:
                    Console.WriteLine("Exception: FLT DENORMAL OPERAND");
                    break;
                case WinApi.EXCEPTION_FLT_DIVIDE_BY_ZERO:
                    Console.WriteLine("Exception: FLT DIVIDE BY ZERO");
                    break;
                case WinApi.EXCEPTION_FLT_INEXACT_RESULT:
                    Console.WriteLine("Exception: FLT INEXACT RESULT");
                    break;
                case WinApi.EXCEPTION_FLT_INVALID_OPERATION:
                    Console.WriteLine("Exception: FLT INVALID OPERATION");
                    break;
                case WinApi.EXCEPTION_FLT_OVERFLOW:
                    Console.WriteLine("Exception: FLT OVERFLOW");
                    break;
                case WinApi.EXCEPTION_FLT_STACK_CHECK:
                    Console.WriteLine("Exception: FLT STACK CHECK");
                    break;
                case WinApi.EXCEPTION_FLT_UNDERFLOW:
                    Console.WriteLine("Exception: FLT UNDERFLOW");
                    break;
                case WinApi.EXCEPTION_ILLEGAL_INSTRUCTION:
                    Console.WriteLine("Exception: ILLEGAL INSTRUCTION");
                    break;
                case WinApi.EXCEPTION_IN_PAGE_ERROR:
                    Console.WriteLine("Exception: IN PAGE ERROR");
                    break;
                case WinApi.EXCEPTION_INT_DIVIDE_BY_ZERO:
                    Console.WriteLine("Exception: INT DIVIDE BY ZERO");
                    break;
                case WinApi.EXCEPTION_INT_OVERFLOW:
                    Console.WriteLine("Exception: INT OVERFLOW");
                    break;
                case WinApi.EXCEPTION_INVALID_DISPOSITION:
                    Console.WriteLine("Exception: INVALID DISPOSITION");
                    break;
                case WinApi.EXCEPTION_NONCONTINUABLE_EXCEPTION:
                    Console.WriteLine("Exception: NONCONTINUABLE EXCEPTION");
                    break;
                case WinApi.EXCEPTION_PRIV_INSTRUCTION:
                    Console.WriteLine("Exception: PRIV INSTRUCTION");
                    break;
                case WinApi.EXCEPTION_SINGLE_STEP:
                    Console.WriteLine("Exception: SINGLE STEP");
                    break;
                case WinApi.EXCEPTION_STACK_OVERFLOW:
                    Console.WriteLine("Exception: STACK OVERFLOW");
                    break;
                default:
                    Console.WriteLine("Exception: ???");
                    break;
                }
            }
            else
            {
                // Already encountered
                // TODO: What to do?
                Console.WriteLine("already encountered exception");
            }
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
            Console.WriteLine($"IP = {WinApi.GetInstructionPointer((Win32Thread)process.MainThread)}");
            var message = info.GetMessage(process.Handle);
            Console.WriteLine($"output string: {message}");
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
                    var thread = threads[debugEvent.dwThreadId];
                    WinApi.ContinueDebugThread(thread, true);
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
            {
                CreateProcessDebugEvent(process, ref debugEvent.u.CreateProcessInfo);
            } break;
            case WinApi.CREATE_THREAD_DEBUG_EVENT:
            {
                CreateThreadDebugEvent(process, ref debugEvent.u.CreateThread);
            } break;
            case WinApi.EXCEPTION_DEBUG_EVENT:
            {
                ExceptionDebugEvent(process, ref debugEvent.u.Exception);
            } break;
            case WinApi.EXIT_PROCESS_DEBUG_EVENT:
            {
                ExitProcessDebugEvent(process, ref debugEvent.u.ExitProcess);
            } break;
            case WinApi.EXIT_THREAD_DEBUG_EVENT:
            {
                ExitThreadDebugEvent(process, ref debugEvent.u.ExitThread);
            } break;
            case WinApi.LOAD_DLL_DEBUG_EVENT:
            {
                LoadDllDebugEvent(process, ref debugEvent.u.LoadDll);
            } break;
            case WinApi.OUTPUT_DEBUG_STRING_EVENT:
            {
                OutputStringDebugEvent(process, ref debugEvent.u.DebugString);
            } break;
            case WinApi.RIP_EVENT:
            {
                RipDebugEvent(process, ref debugEvent.u.RipInfo);
            } break;
            case WinApi.UNLOAD_DLL_DEBUG_EVENT:
            {
                UnloadDllDebugEvent(process, ref debugEvent.u.UnloadDll);
            } break;

            default: throw new NotImplementedException();
            }
        }
    }
}
