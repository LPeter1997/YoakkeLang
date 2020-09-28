using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.DataStructures;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Status;
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
            $"{Return.ToTypeString()} proc[callconv = {CallConv}] {Name}({string.Join(", ", Parameters)}) " +
            $"[visibility = {Visibility}]:\n" +
            $"{string.Join('\n', BasicBlocks)}";

        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Proc), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;

        public void Validate(BuildStatus status)
        {
            foreach (var bb in BasicBlocks)
            {
                if (bb.Proc != this)
                {
                    status.Report(new ValidationError(bb, "The basic block is not linked to the containing procedure!"));
                }
                bb.Validate(status);
            }
        }

        /// <summary>
        /// Checks, if this procedure contains <see cref="Value.User"/>s or <see cref="Type.User"/>s.
        /// If so, the procedure is not appropriate for backend compilation and can only be executed in the VM.
        /// </summary>
        /// <returns>True, if the procedure contains any <see cref="Value.User"/>s or <see cref="Type.User"/>s.</returns>
        public bool HasUserValues()
        {
            // Parameter and return type
            if (Parameters.Select(p => p.Type).Any(t => t.Equals(Type.User_))) return true;
            if (Return.Equals(Type.User_)) return true;
            // Instruction arguments
            var instrArgs = BasicBlocks
                .SelectMany(bb => bb.Instructions)
                .SelectMany(ins => ins.InstrArgs);
            if (instrArgs.Any(arg => arg.Equals(Type.User_) || arg is Value.User)) return true;
            return false;
        }
    }
}
