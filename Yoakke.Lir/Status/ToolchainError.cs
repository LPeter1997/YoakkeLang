using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Reporting;

namespace Yoakke.Lir.Status
{
    /// <summary>
    /// A toolchain error.
    /// </summary>
    public class ToolchainError : IBuildError
    {
        public readonly ITool Tool;
        public readonly string Command;
        public readonly string Message;

        public ToolchainError(ITool tool, string command, string message)
        {
            Tool = tool;
            Command = command;
            Message = message;
        }

        public Diagnostic GetDiagnostic() => new Diagnostic
        {
            Severity = Severity.Error,
            Message = $"Toolchain error while executing {Tool} using the command '{Command}':\n{Message}",
        };
    }
}
