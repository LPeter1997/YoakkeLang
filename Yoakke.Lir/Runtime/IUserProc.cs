using System.Collections.Generic;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Runtime
{
    /// <summary>
    /// Interface for external user-procedures.
    /// </summary>
    public interface IUserProc
    {
        /// <summary>
        /// The return <see cref="Type"/> the procedure returns.
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// Executes the user procedure.
        /// </summary>
        /// <param name="vm">The executing <see cref="VirtualMachine"/>.</param>
        /// <param name="args">The passed in arguments.</param>
        /// <returns>The resulting <see cref="Value"/>.</returns>
        public Value Execute(VirtualMachine vm, IEnumerable<Value> args);
    }
}
