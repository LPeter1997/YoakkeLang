using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Thread : IThread
    {
        public IProcess Process { get; }
        public readonly HANDLE Handle;

        public Win32Thread(IProcess process, HANDLE handle)
        {
            Process = process;
            Handle = handle;
        }
    }
}
