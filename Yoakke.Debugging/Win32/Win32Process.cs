using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Process : IProcess
    {
        public readonly IntPtr Handle;
        public readonly UInt32 Id;
        public Win32ProcessState State { get; set; } = Win32ProcessState.Running;
        private readonly List<Win32Thread> WThreads = new List<Win32Thread>();

        public IThread MainThread => WThreads[0];
        // TODO: Non-thread-safe because of List
        public IReadOnlyCollection<IThread> Threads => WThreads;

        public Win32Process(IntPtr handle, UInt32 id)
        {
            Handle = handle;
            Id = id;
        }

        public Win32Thread GetThread(UInt32 threadId) => WThreads.First(t => t.Id == threadId);

        public void AddThread(Win32Thread thread)
        {
            Debug.Assert(thread.Win32Process == null);
            WThreads.Add(thread);
            thread.Win32Process = this;
        }
    }
}
