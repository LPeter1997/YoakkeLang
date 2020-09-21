using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// An IR procedure.
    /// </summary>
    public class Proc : Value, ISymbol, IValidate
    {
        internal static readonly Proc Null = new Proc(" <null> ");

        public override Type Type =>
            new Type.Proc(CallConv, Return, Parameters.Select(p => p.Type).ToList().AsValueList());
        public string Name { get; }
        public Visibility Visibility { get; set; }

        // TODO: Procedure alignment
        /// <summary>
        /// The return <see cref="Type"/> of the procedure.
        /// </summary>
        public Type Return { get; set; } = Type.Void_;
        /// <summary>
        /// The parameters of this procedure.
        /// </summary>
        public readonly IList<Register> Parameters = new List<Register>();
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

        public override string ToValueString() => Name;

        /// <summary>
        /// Calculates the number of registers allocated by this procedure.
        /// </summary>
        /// <returns>The number of procedures needed by this procedure.</returns>
        public int GetRegisterCount() => Parameters
            .Select(p => p.Index)
            .Concat(BasicBlocks
                .SelectMany(bb => bb.Instructions)
                .Where(ins => ins is ValueInstr)
                // NOTE: Cast returned a nullable for some reason
                .Select(i => (ValueInstr)i)
                .Select(vi => vi.Result.Index))
            .DefaultIfEmpty(-1)
            .Max() + 1;

        public override string ToString() => 
            $"{Return} proc[callconv = {CallConv}] {Name}({string.Join(", ", Parameters)}) " +
            $"[visibility = {Visibility}]:\n" +
            $"{string.Join('\n', BasicBlocks)}";

        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Proc), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;

        public void Validate()
        {
            foreach (var bb in BasicBlocks) bb.Validate();
        }
    }
}
