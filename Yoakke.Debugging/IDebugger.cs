using System;
using Yoakke.Debugging.Events;

namespace Yoakke.Debugging
{
    /// <summary>
    /// Interface for debugging processes.
    /// </summary>
    public interface IDebugger : IDisposable
    {
        /// <summary>
        /// Starts a new process for debugging.
        /// </summary>
        /// <param name="path">The path to the executable.</param>
        /// <param name="args">The arguments to the executable.</param>
        /// <returns>The debugged process.</returns>
        public IProcess StartProcess(string path, string? args = null);
    }
}
