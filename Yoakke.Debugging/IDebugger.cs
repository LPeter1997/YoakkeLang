using System;

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

        /// <summary>
        /// Continues the execution of the given process, if it's in a suspended state.
        /// </summary>
        /// <param name="process">The process to continue executing.</param>
        public void ContinueProcess(IProcess process);

        /// <summary>
        /// Steps the given process a single instruction.
        /// </summary>
        /// <param name="process">The process to step.</param>
        public void StepProcess(IProcess process);

        /// <summary>
        /// Creates a debugger soutable for the current platform.
        /// </summary>
        /// <returns>A new debugger instance.</returns>
        public static IDebugger Create()
        {
            return new Win32.Win32Debugger();
        }
    }
}
