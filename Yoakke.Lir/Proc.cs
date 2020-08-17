﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;

namespace Yoakke.Lir
{
    /// <summary>
    /// An IR procedure.
    /// </summary>
    public class Proc
    {
        /// <summary>
        /// The name of the <see cref="Proc"/>.
        /// </summary>
        public readonly string Name;
        // TODO: Change this to default to void
        /// <summary>
        /// The return <see cref="Type"/> of the procedure.
        /// </summary>
        public Type Return { get; set; } = new Type.Int(true, 32);
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
            BasicBlocks.Add(new BasicBlock("begin"));
        }

        public override string ToString() => 
            $"{Return} proc[callconv = {CallConv.ToString().ToLower()}] {Name}({string.Join(", ", Parameters)}):\n" +
            $"{string.Join('\n', BasicBlocks)}";
    }
}