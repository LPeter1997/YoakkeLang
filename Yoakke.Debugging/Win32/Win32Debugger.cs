using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yoakke.Debugging.Win32
{
    internal class Win32Debugger : IDebugger
    {
        // Threading, disposition state
        private bool disposed = false;
        private volatile bool running = false;
        private Thread loopThread;
        // Communication between threads
        private BlockingCollection<Action> queuedActions;

        public Win32Debugger()
        {
            loopThread = new Thread(RunLoop);
            queuedActions = new BlockingCollection<Action>();
            // NOTE: Start the thread last so everything will be initialized
            loopThread.Start();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            running = false;
            loopThread.Join();
        }

        public IProcess StartProcess(string path, string? args = null)
        {
            var tcs = new TaskCompletionSource<IProcess?>();
            queuedActions.Add(() =>
            {
                var process = WinApi.CreateDebuggableProcess(path, args);
                tcs.SetResult(process);
            });
            var result = tcs.Task.Result;
            if (result == null)
            {
                // TODO: Proper error
                throw new NotImplementedException();
            }
            return result;
        }

        private void RunLoop()
        {
            running = true;
            while (running)
            {
                // First check if there's something to perform and perform them
                for (; queuedActions.TryTake(out var action); action()) ;
                // Now handle debug events
                // TODO
                Thread.Sleep(0);
            }
        }
    }
}
