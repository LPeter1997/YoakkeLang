using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Breakpoint : IBreakpoint
    {
        private Win32Process process;

        public ulong Address { get; }

        private bool isSet = false;
        public bool IsSet 
        { 
            get => isSet; 
            set
            {
                if (isSet == value) return;

                isSet = value;
                if (isSet) Set();
                else Unset();
            }
        }
        private byte buffer;

        public Win32Breakpoint(Win32Process process, ulong address)
        {
            Address = (ulong)process.StartAddress + address;
            this.process = process;
        }

        private void Set()
        {
            unsafe
            {
                // Read out the byte that is replaced
                fixed (byte* pbuffer = &buffer)
                {
                    PInvoke.ReadProcessMemory(process.Handle, (void*)Address, pbuffer, 1, null);
                }
                // Replace with breakpoint
                byte breakpointByte = 0xCC;
                PInvoke.WriteProcessMemory(process.Handle, (void*)Address, &breakpointByte, 1, null);
                // Flush cache
                PInvoke.FlushInstructionCache(process.Handle, (void*)Address, 1);
            }
        }

        private void Unset()
        {

        }
    }
}
