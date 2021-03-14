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

        public void HandleDebugEvent(ref WinApi.DEBUG_EVENT debugEvent)
        {
            switch (debugEvent.dwDebugEventCode)
            {
            case WinApi.CREATE_THREAD_DEBUG_EVENT:
                CreateThreadDebugEvent(ref debugEvent.u.CreateThread);
                break;
            case WinApi.EXIT_THREAD_DEBUG_EVENT:
                ExitThreadDebugEvent(ref debugEvent.u.ExitThread);
                break;
            case WinApi.LOAD_DLL_DEBUG_EVENT:
                LoadDllDebugEvent(ref debugEvent.u.LoadDll);
                break;
            case WinApi.UNLOAD_DLL_DEBUG_EVENT:
                UnloadDllDebugEvent(ref debugEvent.u.UnloadDll);
                break;
            case WinApi.EXCEPTION_DEBUG_EVENT:
                ExceptionDebugEvent(ref debugEvent.u.Exception);
                break;
            case WinApi.OUTPUT_DEBUG_STRING_EVENT:
                OutputStringDebugEvent(ref debugEvent.u.DebugString);
                break;
            case WinApi.RIP_EVENT:
                RipDebugEvent(ref debugEvent.u.RipInfo);
                break;
            default: 
                throw new NotImplementedException();
            }
            WinApi.ContinueDebugThread(GetThread(debugEvent.dwThreadId), true);
        }

        private void CreateThreadDebugEvent(ref WinApi.CREATE_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine("create thread");
            var thread = WinApi.MakeThreadObject(info.hThread);
            AddThread(thread);
        }

        private void ExitThreadDebugEvent(ref WinApi.EXIT_THREAD_DEBUG_INFO info)
        {
            Console.WriteLine("exit thread");
        }

        private void LoadDllDebugEvent(ref WinApi.LOAD_DLL_DEBUG_INFO info)
        {
            Console.WriteLine("load dll");
        }

        private void UnloadDllDebugEvent(ref WinApi.UNLOAD_DLL_DEBUG_INFO info)
        {
            Console.WriteLine("unload dll");
        }

        private void ExceptionDebugEvent(ref WinApi.EXCEPTION_DEBUG_INFO info)
        {
            Console.WriteLine($"IP = {WinApi.GetInstructionPointer(WMainThread)}");
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

        private void OutputStringDebugEvent(ref WinApi.OUTPUT_DEBUG_STRING_INFO info)
        {
            Console.WriteLine($"IP = {WinApi.GetInstructionPointer(WMainThread)}");
            var message = info.GetMessage(Handle);
            Console.WriteLine($"output string: {message}");
        }

        private void RipDebugEvent(ref WinApi.RIP_INFO info)
        {
            Console.WriteLine("RIP");
        }
    }
}
