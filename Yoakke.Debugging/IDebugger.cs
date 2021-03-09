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

        // Events //////////////////////////////////////////////////////////////

        /// <summary>
        /// The event that happens when a process is started.
        /// </summary>
        public event ProcessStartedEventHandler? ProcessStartedEvent;

        /// <summary>
        /// The event that happens when a process is terminated.
        /// </summary>
        public event ProcessTerminatedEventHandler? ProcessTerminatedEvent;

        // API /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a new process for debugging.
        /// </summary>
        /// <param name="path">The path to the executable to start.</param>
        /// <param name="commandLine">The command line arguments to pass.</param>
        /// <returns>The started process.</returns>
        public IProcess StartProcess(string path, string? commandLine);

        // Default for the platform ////////////////////////////////////////////

        public static IDebugger Create()
        {
            return new Win32.Win32Debugger();
        }
    }
}
