using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace Yoakke.Debugging.Win32
{
    internal static class WinApi
    {
        internal const uint MAX_SYM_NAME = 2000;

        internal struct SYMBOL_INFOW
        {
            public UInt32 SizeOfStruct;
            public UInt32 TypeIndex;
            public UInt64 Reserved_1;
            public UInt64 Reserved_2;
            public UInt32 Index;
            public UInt32 Size;
            public UInt64 ModBase;
            public UInt32 Flags;
            public UInt64 Value;
            public UInt64 Address;
            public UInt32 Register;
            public UInt32 Scope;
            public UInt32 Tag;
            public UInt32 NameLen;
            public UInt32 MaxNameLen;
            public Int16 Name;
        }

        [DllImport("Dbghelp", ExactSpelling = true, SetLastError = true)]
        internal static unsafe extern BOOL SymInitializeW(HANDLE hProcess, PCWSTR UserSearchPath, BOOL fInvadeProcess);

        [DllImport("Dbghelp", ExactSpelling = true, SetLastError = true)]
        internal static unsafe extern BOOL SymFromNameW(HANDLE hProcess, PCWSTR name, SYMBOL_INFOW* symbol);

        internal static unsafe BOOL SymFromNameW(HANDLE hProcess, string name, out SYMBOL_INFOW symbol)
        {
            fixed (char* pname = name)
            fixed (SYMBOL_INFOW* psymbol = &symbol)
            {
                return SymFromNameW(hProcess, pname, psymbol);
            }
        }
    }
}
