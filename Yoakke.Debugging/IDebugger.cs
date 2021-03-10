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
        /// The event that happens when a process is terminated.
        /// </summary>
        public event ThreadTerminatedEventHandler? ThreadTerminated;

        // API /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a new process for debugging.
        /// </summary>
        /// <param name="path">The path to the executable to start.</param>
        /// <param name="commandLine">The command line arguments to pass.</param>
        public void StartProcess(string path, string? commandLine);

        // Default for the platform ////////////////////////////////////////////

        public static IDebugger Create()
        {
            return new Win32.Win32Debugger();
        }
    }
}
