﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal static class WinApi
    {
        internal static Win32Process CreateDebuggableProcess(string applicationName, string? commandLine)
        {
            unsafe
            {
                var startupInfo = new STARTUPINFOW();
                startupInfo.cb = (UInt32)sizeof(STARTUPINFOW);
                var processInfo = new PROCESS_INFORMATION();
                Int32 success = FALSE;
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
                if (success == FALSE)
                {
                    // TODO: Error
                    throw new NotImplementedException();
                }
                var handleId = GetProcessId(processInfo.hProcess);
                if (handleId == FALSE)
                {
                    // TODO: Error
                    throw new NotImplementedException();
                }
                return new Win32Process(processInfo.hProcess, handleId);
            }
        }

        internal static bool TryGetDebugEvent(out DEBUG_EVENT debugEvent)
        {
            unsafe
            {
                Int32 success = FALSE;
                fixed (DEBUG_EVENT* lpDebugEvent = &debugEvent)
                {
                    success = WaitForDebugEventEx(lpDebugEvent, 0);
                }
                if (success == FALSE)
                {
                    var err = GetLastError();
                    if (err != ERROR_SEM_TIMEOUT)
                    {
                        // Unexpected
                        // TODO: Proper error
                        throw new NotImplementedException($"err {err}");
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
                Int32 success = ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, DBG_CONTINUE);
                if (success == FALSE)
                {
                    var err = GetLastError();
                    // TODO: Proper error
                    throw new NotImplementedException($"error {err}");
                }
            }
        }

        internal static byte[] ReadInstructionBytes(Win32Process process, ulong offset, int size)
        {
            unsafe
            {
                Int32 success = FALSE;
                byte[] buffer = new byte[size];
                fixed (byte* pBuffer = buffer)
                {
                    // TODO: Offset is wrong here!
                    success = ReadProcessMemory(process.Handle, (void*)offset, pBuffer, (nuint)size, null);
                }
                if (success == FALSE)
                {
                    // TODO: Error
                    var err = GetLastError();
                    throw new NotImplementedException($"error {err}");
                }
                return buffer;
            }
        }

        internal static void WriteInstructionBytes(Win32Process process, ulong offset, byte[] bytes)
        {
            unsafe
            {
                // First write the memory
                Int32 success = FALSE;
                fixed (byte* pBuffer = bytes)
                {
                    // TODO: Offset is wrong here!
                    success = WriteProcessMemory(process.Handle, (void*)offset, pBuffer, (nuint)bytes.Length, null);
                }
                if (success == FALSE)
                {
                    // TODO: Error
                    var err = GetLastError();
                    throw new NotImplementedException($"error {err}");
                }
                // Then flush instruction cache
                success = FlushInstructionCache(process.Handle, (void*)offset, (nuint)bytes.Length);
                if (success == FALSE)
                {
                    // TODO: Error
                    var err = GetLastError();
                    throw new NotImplementedException($"error {err}");
                }
            }
        }

        private const Int32 FALSE = 0;
        private const UInt32 DEBUG_ONLY_THIS_PROCESS = 2;
        private const UInt32 EXCEPTION_MAXIMUM_PARAMETERS = 15;
        private const UInt32 ERROR_SUCCESS = 0;
        private const UInt32 ERROR_SEM_TIMEOUT = 121;
        private const UInt32 DBG_CONTINUE = 0x00010002;

        internal const UInt32 CREATE_PROCESS_DEBUG_EVENT = 3;
        internal const UInt32 CREATE_THREAD_DEBUG_EVENT = 2;
        internal const UInt32 EXCEPTION_DEBUG_EVENT = 1;
        internal const UInt32 EXIT_PROCESS_DEBUG_EVENT = 5;
        internal const UInt32 EXIT_THREAD_DEBUG_EVENT = 4;
        internal const UInt32 LOAD_DLL_DEBUG_EVENT = 6;
        internal const UInt32 OUTPUT_DEBUG_STRING_EVENT = 8;
        internal const UInt32 RIP_EVENT = 9;
        internal const UInt32 UNLOAD_DLL_DEBUG_EVENT = 7;

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

            public string GetMessage(IntPtr hProcess)
            {
                byte[] buffer = new byte[nDebugStringLength * 2];
                Int32 result = 0;
                fixed (byte* pbuffer = buffer)
                {
                    result = ReadProcessMemory(hProcess, lpDebugStringData, pbuffer, nDebugStringLength, null);
                }
                if (result == 0)
                {
                    // TODO: Error
                    var err = GetLastError();
                    throw new NotImplementedException($"error {err}");
                }
                if (fUnicode == 0)
                {
                    // ANSI
                    return Encoding.ASCII.GetString(buffer);
                }
                else
                {
                    // Unicode
                    return Encoding.Unicode.GetString(buffer);
                }
            }
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
            public ExceptionInformation_array ExceptionInformation;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal unsafe struct ExceptionInformation_array
            {
                public nuint Element_0;
                public nuint Element_1;
                public nuint Element_2;
                public nuint Element_3;
                public nuint Element_4;
                public nuint Element_5;
                public nuint Element_6;
                public nuint Element_7;
                public nuint Element_8;
                public nuint Element_9;
                public nuint Element_10;
                public nuint Element_11;
                public nuint Element_12;
                public nuint Element_13;
                public nuint Element_14;
            }
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

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 ReadProcessMemory(
            IntPtr hProcess,
            void* lpBaseAddress,
            void* lpBuffer,
            nuint nSize,
            nuint* lpNumberOfBytesRead);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 WriteProcessMemory(
            IntPtr hProcess,
            void* lpBaseAddress,
            void* lpBuffer,
            nuint nSize,
            nuint* lpNumberOfBytesRead);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 FlushInstructionCache(
            IntPtr hProcess,
            void* lpBaseAddress,
            nuint dwSize);
    }
}
