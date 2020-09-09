using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;

namespace Yoakke.Lir
{
    /// <summary>
    /// An IR procedure.
    /// </summary>
    public class Proc : ISymbol
    {
        public Type Type =>
            new Type.Proc(CallConv, Return, Parameters.Select(p => p.Type).ToList().AsValueList());
        public string Name { get; }
        public Visibility Visibility { get; set; }

        // TODO: Procedure alignment
        // TODO: Change this to default to void
        /// <summary>
        /// The return <see cref="Type"/> of the procedure.
        /// </summary>
        public Type Return { get; set; } = Type.Void_;
        /// <summary>
        /// The parameters of this procedure.
        /// </summary>
        public readonly IList<Register> Parameters = new List<Register>();
        // TODO: Add utility to get type.
        /// <summary>
        /// The calling convention to use when calling this procedure.
        /// </summary>
        public CallConv CallConv { get; set; } = CallConv.Default;
        /// <summary>
        /// The list of <see cref="BasicBlock"/>s the procedure consists of.
        /// </summary>
        public readonly IList<BasicBlock> BasicBlocks = new List<BasicBlock>();

        /// <summary>
        /// Initializes a new <see cref="Proc"/>.
        /// </summary>
        /// <param name="name">The name of the procedure.</param>
        public Proc(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Calculates the number of registers allocated by this procedure.
        /// </summary>
        /// <returns>The number of procedures needed by this procedure.</returns>
        public int GetRegisterCount() => Parameters
            .Select(p => p.Index)
            .Concat(BasicBlocks
                .SelectMany(bb => bb.Instructions)
                .Where(ins => ins is ValueInstr)
                .Cast<ValueInstr>()
                .Select(vi => vi.Result.Index))
            .DefaultIfEmpty(-1)
            .Max() + 1;

        public override string ToString() => 
            $"{Return} proc[callconv = {CallConv}] {Name}({string.Join(", ", Parameters)}) " +
            $"[visibility = {Visibility}]:\n" +
            $"{string.Join('\n', BasicBlocks)}";
    }
}
