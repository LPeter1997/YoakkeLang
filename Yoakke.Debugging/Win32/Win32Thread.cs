using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Thread : IThread
    {
        public readonly IntPtr Handle;
        public readonly UInt32 Id;
        public Win32Process Win32Process { get; set; }

        public IProcess Process => Win32Process;

        public Win32Thread(IntPtr handle, UInt32 id)
        {
            Handle = handle;
            Id = id;
        }
    }
}
