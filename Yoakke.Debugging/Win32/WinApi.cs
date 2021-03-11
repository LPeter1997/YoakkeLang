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

        private const Int32 FALSE = 0;
        private const UInt32 DEBUG_ONLY_THIS_PROCESS = 2;

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

        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public UInt32 dwProcessId;
            public UInt32 dwThreadId;
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
    }
}
