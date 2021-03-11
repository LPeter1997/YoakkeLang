using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                ErrorOnFalse(success);
                var handleId = GetProcessId(processInfo.hProcess);
                ErrorOnFalse((Int32)handleId);
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
                        ThrowApiError(err);
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
                ErrorOnFalse(success);
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
                ErrorOnFalse(success);
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
                ErrorOnFalse(success);
                // Then flush instruction cache
                success = FlushInstructionCache(process.Handle, (void*)offset, (nuint)bytes.Length);
                ErrorOnFalse(success);
            }
        }

        internal static void OffsetInstructionPointer(IntPtr threadHandle, int offset)
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            unsafe
            {
                if (arch == Architecture.X86)
                {
                    var context = new CONTEXT_X86();
                    context.ContextFlags = 1;
                    var success = GetThreadContext_X86(threadHandle, &context);
                    ErrorOnFalse(success);
                    context.Eip = (uint)((int)context.Eip + offset);
                    success = SetThreadContext_X86(threadHandle, &context);
                    ErrorOnFalse(success);
                }
                else
                {
                    throw new NotImplementedException($"Architecture {arch} does not support this operation");
                }
            }
        }

        private static void ErrorOnFalse(Int32 success)
        {
            if (success == 0)
            {
                var err = GetLastError();
                ThrowApiError(err);
            }
        }

        private static void ThrowApiError(UInt32 errorCode)
        {
            throw new InvalidOperationException($"WinAPI error code {errorCode}");
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
                Int32 success = 0;
                fixed (byte* pbuffer = buffer)
                {
                    success = ReadProcessMemory(hProcess, lpDebugStringData, pbuffer, nDebugStringLength, null);
                }
                ErrorOnFalse(success);
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

        internal unsafe struct CONTEXT_X86
        {
            public UInt32 ContextFlags;
            public UInt32 Dr0;
            public UInt32 Dr1;
            public UInt32 Dr2;
            public UInt32 Dr3;
            public UInt32 Dr6;
            public UInt32 Dr7;
            public FLOATING_SAVE_AREA_X86 FloatSave;
            public UInt32 SegGs;
            public UInt32 SegFs;
            public UInt32 SegEs;
            public UInt32 SegDs;
            public UInt32 Edi;
            public UInt32 Esi;
            public UInt32 Ebx;
            public UInt32 Edx;
            public UInt32 Ecx;
            public UInt32 Eax;
            public UInt32 Ebp;
            public UInt32 Eip;
            public UInt32 SegCs;
            public UInt32 EFlags;
            public UInt32 Esp;
            public UInt32 SegSs;
            public ExtendedRegisters_array ExtendedRegisters;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal unsafe struct ExtendedRegisters_array
            {
                byte Element_0, Element_1, Element_2, Element_3, Element_4, Element_5, Element_6, Element_7, Element_8, Element_9, Element_10, Element_11, Element_12, Element_13, Element_14, Element_15, Element_16, Element_17, Element_18, Element_19, Element_20, Element_21, Element_22, Element_23, Element_24, Element_25, Element_26, Element_27, Element_28, Element_29, Element_30, Element_31, Element_32, Element_33, Element_34, Element_35, Element_36, Element_37, Element_38, Element_39, Element_40, Element_41, Element_42, Element_43, Element_44, Element_45, Element_46, Element_47, Element_48, Element_49, Element_50, Element_51, Element_52, Element_53, Element_54, Element_55, Element_56, Element_57, Element_58, Element_59, Element_60, Element_61, Element_62, Element_63, 
                    Element_64, Element_65, Element_66, Element_67, Element_68, Element_69, Element_70, Element_71, Element_72, Element_73, Element_74, Element_75, Element_76, Element_77, Element_78, Element_79, Element_80, Element_81, Element_82, Element_83, Element_84, Element_85, Element_86, Element_87, Element_88, Element_89, Element_90, Element_91, Element_92, Element_93, Element_94, Element_95, Element_96, Element_97, Element_98, Element_99, Element_100, Element_101, Element_102, Element_103, Element_104, Element_105, Element_106, Element_107, Element_108, Element_109, Element_110, Element_111, Element_112, Element_113, Element_114, Element_115, Element_116, Element_117, Element_118, Element_119, Element_120, Element_121, Element_122, Element_123, Element_124, Element_125, Element_126, Element_127, 
                    Element_128, Element_129, Element_130, Element_131, Element_132, Element_133, Element_134, Element_135, Element_136, Element_137, Element_138, Element_139, Element_140, Element_141, Element_142, Element_143, Element_144, Element_145, Element_146, Element_147, Element_148, Element_149, Element_150, Element_151, Element_152, Element_153, Element_154, Element_155, Element_156, Element_157, Element_158, Element_159, Element_160, Element_161, Element_162, Element_163, Element_164, Element_165, Element_166, Element_167, Element_168, Element_169, Element_170, Element_171, Element_172, Element_173, Element_174, Element_175, Element_176, Element_177, Element_178, Element_179, Element_180, Element_181, Element_182, Element_183, Element_184, Element_185, Element_186, Element_187, Element_188, Element_189, Element_190, Element_191, 
                    Element_192, Element_193, Element_194, Element_195, Element_196, Element_197, Element_198, Element_199, Element_200, Element_201, Element_202, Element_203, Element_204, Element_205, Element_206, Element_207, Element_208, Element_209, Element_210, Element_211, Element_212, Element_213, Element_214, Element_215, Element_216, Element_217, Element_218, Element_219, Element_220, Element_221, Element_222, Element_223, Element_224, Element_225, Element_226, Element_227, Element_228, Element_229, Element_230, Element_231, Element_232, Element_233, Element_234, Element_235, Element_236, Element_237, Element_238, Element_239, Element_240, Element_241, Element_242, Element_243, Element_244, Element_245, Element_246, Element_247, Element_248, Element_249, Element_250, Element_251, Element_252, Element_253, Element_254, Element_255, 
                    Element_256, Element_257, Element_258, Element_259, Element_260, Element_261, Element_262, Element_263, Element_264, Element_265, Element_266, Element_267, Element_268, Element_269, Element_270, Element_271, Element_272, Element_273, Element_274, Element_275, Element_276, Element_277, Element_278, Element_279, Element_280, Element_281, Element_282, Element_283, Element_284, Element_285, Element_286, Element_287, Element_288, Element_289, Element_290, Element_291, Element_292, Element_293, Element_294, Element_295, Element_296, Element_297, Element_298, Element_299, Element_300, Element_301, Element_302, Element_303, Element_304, Element_305, Element_306, Element_307, Element_308, Element_309, Element_310, Element_311, Element_312, Element_313, Element_314, Element_315, Element_316, Element_317, Element_318, Element_319, 
                    Element_320, Element_321, Element_322, Element_323, Element_324, Element_325, Element_326, Element_327, Element_328, Element_329, Element_330, Element_331, Element_332, Element_333, Element_334, Element_335, Element_336, Element_337, Element_338, Element_339, Element_340, Element_341, Element_342, Element_343, Element_344, Element_345, Element_346, Element_347, Element_348, Element_349, Element_350, Element_351, Element_352, Element_353, Element_354, Element_355, Element_356, Element_357, Element_358, Element_359, Element_360, Element_361, Element_362, Element_363, Element_364, Element_365, Element_366, Element_367, Element_368, Element_369, Element_370, Element_371, Element_372, Element_373, Element_374, Element_375, Element_376, Element_377, Element_378, Element_379, Element_380, Element_381, Element_382, Element_383, 
                    Element_384, Element_385, Element_386, Element_387, Element_388, Element_389, Element_390, Element_391, Element_392, Element_393, Element_394, Element_395, Element_396, Element_397, Element_398, Element_399, Element_400, Element_401, Element_402, Element_403, Element_404, Element_405, Element_406, Element_407, Element_408, Element_409, Element_410, Element_411, Element_412, Element_413, Element_414, Element_415, Element_416, Element_417, Element_418, Element_419, Element_420, Element_421, Element_422, Element_423, Element_424, Element_425, Element_426, Element_427, Element_428, Element_429, Element_430, Element_431, Element_432, Element_433, Element_434, Element_435, Element_436, Element_437, Element_438, Element_439, Element_440, Element_441, Element_442, Element_443, Element_444, Element_445, Element_446, Element_447, 
                    Element_448, Element_449, Element_450, Element_451, Element_452, Element_453, Element_454, Element_455, Element_456, Element_457, Element_458, Element_459, Element_460, Element_461, Element_462, Element_463, Element_464, Element_465, Element_466, Element_467, Element_468, Element_469, Element_470, Element_471, Element_472, Element_473, Element_474, Element_475, Element_476, Element_477, Element_478, Element_479, Element_480, Element_481, Element_482, Element_483, Element_484, Element_485, Element_486, Element_487, Element_488, Element_489, Element_490, Element_491, Element_492, Element_493, Element_494, Element_495, Element_496, Element_497, Element_498, Element_499, Element_500, Element_501, Element_502, Element_503, Element_504, Element_505, Element_506, Element_507, Element_508, Element_509, Element_510, Element_511;
            }
        }

        internal unsafe struct FLOATING_SAVE_AREA_X86
        {
            public UInt32 ControlWord;
            public UInt32 StatusWord;
            public UInt32 TagWord;
            public UInt32 ErrorOffset;
            public UInt32 ErrorSelector;
            public UInt32 DataOffset;
            public UInt32 DataSelector;
            public RegisterArea_array RegisterArea;
            public UInt32 Spare0;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal unsafe struct RegisterArea_array
            {
                byte Element_0, Element_1, Element_2, Element_3, Element_4, Element_5, Element_6, Element_7, Element_8, Element_9, Element_10, Element_11, Element_12, Element_13, Element_14, Element_15, Element_16, Element_17, Element_18, Element_19, 
                    Element_20, Element_21, Element_22, Element_23, Element_24, Element_25, Element_26, Element_27, Element_28, Element_29, Element_30, Element_31, Element_32, Element_33, Element_34, Element_35, Element_36, Element_37, Element_38, Element_39, 
                    Element_40, Element_41, Element_42, Element_43, Element_44, Element_45, Element_46, Element_47, Element_48, Element_49, Element_50, Element_51, Element_52, Element_53, Element_54, Element_55, Element_56, Element_57, Element_58, Element_59, 
                    Element_60, Element_61, Element_62, Element_63, Element_64, Element_65, Element_66, Element_67, Element_68, Element_69, Element_70, Element_71, Element_72, Element_73, Element_74, Element_75, Element_76, Element_77, Element_78, Element_79;
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

        [DllImport("Kernel32", EntryPoint = "GetThreadContext", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 GetThreadContext_X86(
            IntPtr hThread,
            CONTEXT_X86* lpContext);

        [DllImport("Kernel32", EntryPoint = "SetThreadContext", ExactSpelling = true, SetLastError = true)]
        private static extern unsafe Int32 SetThreadContext_X86(
            IntPtr hThread,
            CONTEXT_X86* lpContext);
    }
}
