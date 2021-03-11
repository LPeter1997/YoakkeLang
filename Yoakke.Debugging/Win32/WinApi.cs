using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal static class WinApi
    {
        internal static Win32Process? CreateDebuggableProcess(string applicationName, string? commandLine)
        {
            unsafe
            {
                var startupInfo = new STARTUPINFOW();
                startupInfo.cb = (UInt32)sizeof(STARTUPINFOW);
                var processInfo = new PROCESS_INFORMATION();
                Int32 success = 0;
                fixed (char* lpApplicationName = applicationName)
                fixed (char* lpCommandLine = commandLine)
                {
                    success = CreateProcessW(
                        lpApplicationName,
                        lpCommandLine,
                        null, null,
                        FALSE,
                        DEBUG_ONLY_THIS_PROCESS,
                        null,
                        null,
                        &startupInfo,
                        &processInfo);
                }
                if (success == FALSE) return null;
                var handleId = GetProcessId(processInfo.hProcess);
                if (handleId == 0) return null;
                return new Win32Process(processInfo.hProcess, handleId);
            }
        }

        internal static bool TryGetDebugEvent(out DEBUG_EVENT debugEvent)
        {
            unsafe
            {
                Int32 result = 0;
                fixed (DEBUG_EVENT* lpDebugEvent = &debugEvent)
                {
                    result = WaitForDebugEventEx(lpDebugEvent, 0);
                }
                if (result == 0)
                {
                    var err = GetLastError();
                    if (err != ERROR_SEM_TIMEOUT)
                    {
                        // Unexpected
                        // TODO: Proper error
                        throw new NotImplementedException();
                    }
                    return false;
                }
                else return true;
            }
        }

        // TODO: Not just DBG_CONTINUE?
        internal static void ContinueDebugEvent(ref DEBUG_EVENT debugEvent)
        {
            unsafe
            {
                Int32 result = 0;
                ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                if (result == 0)
                {
                    var err = GetLastError();
                    if (err != ERROR_SUCCESS)
                    {
                        // TODO: Proper error
                        throw new NotImplementedException($"error {err}");
                    }
                }
            }
        }

        private const Int32 FALSE = 0;
        private const UInt32 DEBUG_ONLY_THIS_PROCESS = 2;
        private const UInt32 EXCEPTION_MAXIMUM_PARAMETERS = 15;
        private const UInt32 ERROR_SUCCESS = 0;
        private const UInt32 ERROR_SEM_TIMEOUT = 121;
        private const UInt32 DBG_CONTINUE = 0x00010002;

        private unsafe struct STARTUPINFOW
        {
            public UInt32 cb;
            public char* lpReserved;
            public char* lpDesktop;
            public char* lpTitle;
            public UInt32 dwX;
            public UInt32 dwY;
            public UInt32 dwXSize;
            public UInt32 dwYSize;
            public UInt32 dwXCountChars;
            public UInt32 dwYCountChars;
            public UInt32 dwFillAttribute;
            public UInt32 dwFlags;
            public UInt16 wShowWindow;
            public UInt16 cbReserved2;
            public byte* lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        private unsafe struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public UInt32 dwProcessId;
            public UInt32 dwThreadId;
        }

        internal unsafe struct DEBUG_EVENT
        {
            public UInt32 dwDebugEventCode;
            public UInt32 dwProcessId;
            public UInt32 dwThreadId;
            public Field_u u;

            [StructLayout(LayoutKind.Explicit)]
            internal unsafe struct Field_u {
                [FieldOffset(0)]
                public EXCEPTION_DEBUG_INFO Exception;
                [FieldOffset(0)]
                public CREATE_THREAD_DEBUG_INFO CreateThread;
                [FieldOffset(0)]
                public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo;
                [FieldOffset(0)]
                public EXIT_THREAD_DEBUG_INFO ExitThread;
                [FieldOffset(0)]
                public EXIT_PROCESS_DEBUG_INFO ExitProcess;
                [FieldOffset(0)]
                public LOAD_DLL_DEBUG_INFO LoadDll;
                [FieldOffset(0)]
                public UNLOAD_DLL_DEBUG_INFO UnloadDll;
                [FieldOffset(0)]
                public OUTPUT_DEBUG_STRING_INFO DebugString;
                [FieldOffset(0)]
                public RIP_INFO RipInfo;
            }
        }

        internal unsafe struct EXCEPTION_DEBUG_INFO
        {
            public EXCEPTION_RECORD ExceptionRecord;
            public UInt32 dwFirstChance;
        }

        internal unsafe struct CREATE_THREAD_DEBUG_INFO
        {
            public IntPtr hThread;
            public void* lpThreadLocalBase;
            public void* lpStartAddress;
        }

        internal unsafe struct CREATE_PROCESS_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr hProcess;
            public IntPtr hThread;
            public void* lpBaseOfImage;
            public UInt32 dwDebugInfoFileOffset;
            public UInt32 nDebugInfoSize;
            public void* lpThreadLocalBase;
            public void* lpStartAddress;
            public void* lpImageName;
            public UInt16 fUnicode;
        }

        internal unsafe struct EXIT_THREAD_DEBUG_INFO
        {
            public UInt32 dwExitCode;
        }

        internal unsafe struct EXIT_PROCESS_DEBUG_INFO
        {
            public UInt32 dwExitCode;
        }

        internal unsafe struct LOAD_DLL_DEBUG_INFO
        {
            public IntPtr hFile;
            public void* lpBaseOfDll;
            public UInt32 dwDebugInfoFileOffset;
            public UInt32 nDebugInfoSize;
            public void* lpImageName;
            public UInt16 fUnicode;
        }

        internal unsafe struct UNLOAD_DLL_DEBUG_INFO
        {
            public void* lpBaseOfDll;
        }

        internal unsafe struct OUTPUT_DEBUG_STRING_INFO
        {
            public byte* lpDebugStringData;
            public UInt16 fUnicode;
            public UInt16 nDebugStringLength;
        }

        internal unsafe struct RIP_INFO
        {
            public UInt32 dwError;
            public UInt32 dwType;
        }

        internal unsafe struct EXCEPTION_RECORD
        {
            public UInt32 ExceptionCode;
            public UInt32 ExceptionFlags;
            public EXCEPTION_RECORD* ExceptionRecord;
            public void* ExceptionAddress;
            public UInt32 NumberParameters;

            public UInt32* ExceptionInformation_0;
            public UInt32* ExceptionInformation_1;
            public UInt32* ExceptionInformation_2;
            public UInt32* ExceptionInformation_3;
            public UInt32* ExceptionInformation_4;
            public UInt32* ExceptionInformation_5;
            public UInt32* ExceptionInformation_6;
            public UInt32* ExceptionInformation_7;
            public UInt32* ExceptionInformation_8;
            public UInt32* ExceptionInformation_9;
            public UInt32* ExceptionInformation_10;
            public UInt32* ExceptionInformation_11;
            public UInt32* ExceptionInformation_12;
            public UInt32* ExceptionInformation_13;
            public UInt32* ExceptionInformation_14;
        }

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe UInt32 GetProcessId(IntPtr process);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 CreateProcessW(
            char* lpApplicationName,
            char* lpCommandLine,
            void* lpProcessAttributes,
            void* lpThreadAttributes,
            Int32 bInheritHandles,
            UInt32 dwCreationFlags,
            void* lpEnvironment,
            char* lpCurrentDirectory,
            STARTUPINFOW* lpStartupInfo,
            PROCESS_INFORMATION* lpProcessInformation);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 WaitForDebugEventEx(
            DEBUG_EVENT* lpDebugEvent,
            UInt32 dwMilliseconds);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 ContinueDebugEvent(
            UInt32 dwProcessId,
            UInt32 dwThreadId,
            UInt32 dwContinueStatus);

        [DllImport("Kernel32", ExactSpelling = true)]
        private static extern unsafe UInt32 GetLastError();
    }
}
