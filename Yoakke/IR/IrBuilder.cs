using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Semantic;
using Yoakke.Utils;

namespace Yoakke.IR
{
    /// <summary>
    /// Helper to compile IR code into an <see cref="Assembly"/>.
    /// </summary>
    class IrBuilder
    {
        /// <summary>
        /// The <see cref="Assembly"/> that holds the compiled IR code.
        /// </summary>
        public readonly Assembly Assembly;

        /// <summary>
        /// The currently compiled <see cref="Proc"/>.
        /// </summary>
        public Proc CurrentProc => Assert.NonNullValue(currentProc);

        /// <summary>
        /// The currently compiled <see cref="BasicBlock"/>.
        /// </summary>
        public BasicBlock CurrentBasicBlock => Assert.NonNullValue(currentBB);

        private Proc? currentProc;
        private BasicBlock? currentBB;

        /// <summary>
        /// Initializes a new <see cref="IrBuilder"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile the IR into.</param>
        public IrBuilder(Assembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Declares an external <see cref="Value"/>.
        /// </summary>
        /// <param name="external">The <see cref="Value.Extern"/> to declare.</param>
        public void DeclareExternal(Value.Extern external)
        {
            Assembly.Externals.Add(external);
        }

        /// <summary>
        /// Compiles a new <see cref="Proc"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of this procedure.</param>
        /// <param name="action">The callback that's being called, when the builder context switches to
        /// this new procedure. After the callback, the context will switch back to the old one.</param>
        /// <returns>The created <see cref="Proc"/>.</returns>
        public Proc CreateProc(Type type, Action action)
        {
            var lastProc = currentProc;
            var lastBB = currentBB;

            var createdProc = new Proc(type);
            currentProc = createdProc;
            Assembly.Procedures.Add(currentProc);
            CreateBasicBlock("begin");

            action();
            
            currentProc = lastProc;
            currentBB = lastBB;

            return createdProc;
        }

        /// <summary>
        /// Createsa new <see cref="BasicBlock"/> in the current procedure.
        /// </summary>
        /// <param name="name">The name of the <see cref="BasicBlock"/> to create.</param>
        public void CreateBasicBlock(string name)
        {
            Assert.NonNull(currentProc);

            currentBB = new BasicBlock();
            currentProc.BasicBlocks.Add(currentBB);
        }

        /// <summary>
        /// Adds an <see cref="Instruction"/> to the current <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="instruction">The <see cref="Instruction"/> to add.</param>
        public void AddInstruction(Instruction instruction)
        {
            Assert.NonNull(currentBB);

            currentBB.Instructions.Add(instruction);
        }
    }
}
