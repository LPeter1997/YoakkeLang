using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Process : IProcess
    {
        public readonly HANDLE Handle;

        public Win32Process(HANDLE handle)
        {
            Handle = handle;
        }
    }
}
