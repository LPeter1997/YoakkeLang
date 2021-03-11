using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Thread : IThread
    {
        public IProcess Process { get; }
        public readonly IntPtr Handle;
        public readonly UInt32 Id;

        public Win32Thread(Win32Process process, IntPtr handle, UInt32 id)
        {
            Process = process;
            Handle = handle;
            Id = id;
        }
    }
}
