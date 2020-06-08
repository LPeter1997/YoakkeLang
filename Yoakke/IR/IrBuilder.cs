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
        public Proc CurrentProc
        {
            get { Assert.NonNull(currentProc); return currentProc; }
        }

        /// <summary>
        /// The currently compiled <see cref="BasicBlock"/>.
        /// </summary>
        public BasicBlock CurrentBasicBlock
        {
            get { Assert.NonNull(currentBB); return currentBB; }
        }

        private HashSet<string> globalNames = new HashSet<string>();
        private Proc? currentProc;
        private BasicBlock? currentBB;
        private HashSet<string>? currentLocalNames;

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
        /// <param name="name">The name of the procedure.</param>
        /// <param name="returnType">The <see cref="Type"/> phe procedure returns.</param>
        /// <param name="action">The callback that's being called, when the builder context switches to
        /// this new procedure. After the callback, the context will switch back to the old one.</param>
        /// <returns>The created <see cref="Proc"/>.</returns>
        public Proc CreateProc(string name, Type returnType, Action action)
        {
            name = GlobalUniqueName(name);

            var lastProc = currentProc;
            var lastBB = currentBB;
            var lastLocalNames = currentLocalNames;

            var createdProc = new Proc(name, returnType); ;
            currentProc = createdProc;
            Assembly.Procedures.Add(currentProc);
            currentLocalNames = new HashSet<string>();
            CreateBasicBlock("begin");

            action();
            
            currentProc = lastProc;
            currentBB = lastBB;
            currentLocalNames = lastLocalNames;

            return createdProc;
        }

        /// <summary>
        /// Createsa new <see cref="BasicBlock"/> in the current procedure.
        /// </summary>
        /// <param name="name">The name of the <see cref="BasicBlock"/> to create.</param>
        public void CreateBasicBlock(string name)
        {
            Assert.NonNull(currentProc);

            name = LocalUniqueName(name);
            currentBB = new BasicBlock(name);
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

        private string LocalUniqueName(string name)
        {
            Assert.NonNull(currentLocalNames);

            if (!globalNames.Contains(name) && currentLocalNames.Add(name)) return name;
            for (int i = 0; ; ++i)
            {
                var numberedName = $"{name}{i}";
                if (!globalNames.Contains(numberedName) 
                  && currentLocalNames.Add(numberedName)) return numberedName;
            }
        }

        private string GlobalUniqueName(string name)
        {
            if (globalNames.Add(name)) return name;
            for (int i = 0; ; ++i)
            {
                var numberedName = $"{name}{i}";
                if (globalNames.Add(numberedName)) return numberedName;
            }
        }
    }
}
