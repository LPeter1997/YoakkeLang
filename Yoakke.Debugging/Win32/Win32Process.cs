using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Process : IProcess
    {
        public readonly IntPtr Handle;
        public readonly UInt32 Id;
        public Win32ProcessState State { get; set; } = Win32ProcessState.Running;
        private readonly List<Win32Thread> WThreads = new List<Win32Thread>();
        public Win32Thread WMainThread => WThreads[0];

        public IThread MainThread => WMainThread;
        // TODO: Non-thread-safe because of List
        public IReadOnlyCollection<IThread> Threads => WThreads;

        // The thread that needs to be continued if a breakpoint occurred
        private Win32Thread? suspendedThread;

        public Win32Process(IntPtr handle, UInt32 id)
        {
            Handle = handle;
            Id = id;
        }

        public Win32Thread GetThread(UInt32 threadId) => WThreads.First(t => t.Id == threadId);

        public void AddThread(Win32Thread thread)
        {
            Debug.Assert(thread.Win32Process == null);
            WThreads.Add(thread);
            thread.Win32Process = this;
        }

        public void ContinueExecution()
        {
            // NO-OP
            if (State == Win32ProcessState.Running) return;

            Debug.Assert(suspendedThread != null);
            State = Win32ProcessState.Running;
            WinApi.ContinueDebugThread(suspendedThread, true);
            suspendedThread = null;
        }

        public void HandleDebugEvent(ref WinApi.DEBUG_EVENT debugEvent)
        {
            switch (debugEvent.dwDebugEventCode)
            {
            case WinApi.CREATE_THREAD_DEBUG_EVENT:
            {
                CreateThreadDebugEvent(ref debugEvent.u.CreateThread);
            } break;
            case WinApi.EXIT_THREAD_DEBUG_EVENT:
            {
                var thread = GetThread(debugEvent.dwThreadId);
                ExitThreadDebugEvent(thread, ref debugEvent.u.ExitThread);
            } break;
            case WinApi.LOAD_DLL_DEBUG_EVENT:
            {
                var thread = GetThread(debugEvent.dwThreadId);
                LoadDllDebugEvent(thread, ref debugEvent.u.LoadDll);
            } break;
            case WinApi.UNLOAD_DLL_DEBUG_EVENT:
            {
                var thread = GetThread(debugEvent.dwThreadId);
                UnloadDllDebugEvent(thread, ref debugEvent.u.UnloadDll);
            } break;
            case WinApi.EXCEPTION_DEBUG_EVENT:
            {
                var thread = GetThread(debugEvent.dwThreadId);
                ExceptionDebugEvent(thread, ref debugEvent.u.Exception);
            } break;
            case WinApi.OUTPUT_DEBUG_STRING_EVENT:
            {
                var thread = GetThread(debugEvent.dwThreadId);
                OutputStringDebugEvent(thread, ref debugEvent.u.DebugString);
            } break;
            case WinApi.RIP_EVENT:
            {
                var thread = GetThread(debugEvent.dwThreadId);
                RipDebugEvent(thread, ref debugEvent.u.RipInfo);
            } break;

            default: 
                throw new NotImplementedException();
            }
        }

        private void CreateThreadDebugEvent(ref WinApi.CREATE_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine("create thread");
            var thread = WinApi.MakeThreadObject(info.hThread);
            AddThread(thread);
            // Trivially continuable, handled
            WinApi.ContinueDebugThread(thread, true);
        }

        private void ExitThreadDebugEvent(Win32Thread thread, ref WinApi.EXIT_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine($"exit thread (code {info.dwExitCode})");
            // Trivially continuable, handled
            WinApi.ContinueDebugThread(thread, true);
        }

        private void LoadDllDebugEvent(Win32Thread thread, ref WinApi.LOAD_DLL_DEBUG_INFO info)
        {
            Console.WriteLine("load dll");
            // Trivially continuable, handled
            WinApi.ContinueDebugThread(thread, true);
        }

        private void UnloadDllDebugEvent(Win32Thread thread, ref WinApi.UNLOAD_DLL_DEBUG_INFO info)
        {
            Console.WriteLine("unload dll");
            // Trivially continuable, handled
            WinApi.ContinueDebugThread(thread, true);
        }

        private void ExceptionDebugEvent(Win32Thread thread, ref WinApi.EXCEPTION_DEBUG_INFO info)
        {
            Console.WriteLine($"IP = {WinApi.GetInstructionPointer(WMainThread)}");
            if (info.dwFirstChance == 0)
            {
                // It's not the first time we encountered this exception, it's an error
                // TODO: Error
                Console.WriteLine("already encountered exception");
                return;
            }
            // First time encounter
            if (info.ExceptionRecord.ExceptionCode == WinApi.EXCEPTION_BREAKPOINT)
            {
                // A breakpoint was hit
                Console.WriteLine("Breakpoint");
                SuspendThread(thread);
            }
            else if (info.ExceptionRecord.ExceptionCode == WinApi.EXCEPTION_SINGLE_STEP)
            {
                // Trap flag was set
                Console.WriteLine("Single step");
                SuspendThread(thread);
            }
            else
            {
                // Some other exception like avvess violation or division by zero
                Console.WriteLine($"Exception: {info.ExceptionRecord.ExceptionCode}");
                // TODO: What to do here?
            }
        }

        private void SuspendThread(Win32Thread thread)
        {
            Debug.Assert(suspendedThread == null);
            State = Win32ProcessState.Suspended;
            suspendedThread = thread;
        }

        private void OutputStringDebugEvent(Win32Thread thread, ref WinApi.OUTPUT_DEBUG_STRING_INFO info)
        {
            Console.WriteLine($"IP = {WinApi.GetInstructionPointer(WMainThread)}");
            var message = info.GetMessage(Handle);
            Console.WriteLine($"output string: {message}");
            // Trivially continuable, handled
            WinApi.ContinueDebugThread(thread, true);
        }

        private void RipDebugEvent(Win32Thread thread, ref WinApi.RIP_INFO info)
        {
            Console.WriteLine("RIP");
            // TODO: What to do here?
        }
    }
}
