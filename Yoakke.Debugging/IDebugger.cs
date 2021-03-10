using System;
using Yoakke.Debugging.Events;

namespace Yoakke.Debugging
{
    /// <summary>
    /// Interface for debugging a process.
    /// </summary>
    public interface IDebugger : IDisposable
    {
        // Delegates ///////////////////////////////////////////////////////////

        /// <summary>
        /// Delegate for the event when a process is started.
        /// </summary>
        /// <param name="sender">The debugger that sent this event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void ProcessStartedEventHandler(object? sender, ProcessStartedEventArgs args);

        /// <summary>
        /// Delegate for the event when a process is terminated.
        /// </summary>
        /// <param name="sender">The debugger that sent this event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void ProcessTerminatedEventHandler(object? sender, ProcessTerminatedEventArgs args);

        /// <summary>
        /// Delegate for the event when a thread is started.
        /// </summary>
        /// <param name="sender">The debugger that sent this event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void ThreadStartedEventHandler(object? sender, ThreadStartedEventArgs args);

        /// <summary>
        /// Delegate for the event when a thread is terminated.
        /// </summary>
        /// <param name="sender">The debugger that sent this event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void ThreadTerminatedEventHandler(object? sender, ThreadTerminatedEventArgs args);

        /// <summary>
        /// Delegate for the event when a debug message is sent.
        /// </summary>
        /// <param name="sender">The debugger that sent this event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void DebugOutputEventHandler(object? sender, DebugOutputEventArgs args);

        // Events //////////////////////////////////////////////////////////////

        /// <summary>
        /// The event that happens when a process is started.
        /// </summary>
        public event ProcessStartedEventHandler? ProcessStarted;

        /// <summary>
        /// The event that happens when a process is terminated.
        /// </summary>
        public event ProcessTerminatedEventHandler? ProcessTerminated;

        /// <summary>
        /// The event that happens when a thread is started.
        /// </summary>
        public event ThreadStartedEventHandler? ThreadStarted;

        /// <summary>
        /// The event that happens when a thread is terminated.
        /// </summary>
        public event ThreadTerminatedEventHandler? ThreadTerminated;

        /// <summary>
        /// The event that happens when a process sends some debug output.
        /// </summary>
        public event DebugOutputEventHandler? DebugOutput;

        // API /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a new process for debugging.
        /// </summary>
        /// <param name="path">The path to the executable to start.</param>
        /// <param name="commandLine">The command line arguments to pass.</param>
        public void StartProcess(string path, string? commandLine);

        /// <summary>
        /// Adds a breakpoint (disabled by default) at the given offset.
        /// </summary>
        /// <param name="process">The process to add the breakpoint to.</param>
        /// <param name="offset">The offset of the breakpoint relative to the process base address.</param>
        /// <returns>The handle to the breakpoint.</returns>
        public IBreakpoint AddBreakpoint(IProcess process, ulong offset);

        // Default for the platform ////////////////////////////////////////////

        public static IDebugger Create()
        {
            return new Win32.Win32Debugger();
        }
    }
}
