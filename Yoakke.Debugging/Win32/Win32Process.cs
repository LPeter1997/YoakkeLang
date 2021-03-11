using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Process : IProcess
    {
        public readonly IntPtr Handle;
        public readonly UInt32 Id;
        public nuint StartAddress { get; internal set; }

        public Win32Process(IntPtr handle, UInt32 id)
        {
            Handle = handle;
            Id = id;
        }
    }
}
