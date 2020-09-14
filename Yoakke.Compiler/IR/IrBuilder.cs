using System.Collections.Generic;
using System.Diagnostics;

namespace Yoakke.Compiler.IR
{
    /// <summary>
    /// Helper to compile IR code into an <see cref="Assembly"/>.
    /// </summary>
    class IrBuilder
    {
        private class ProcContext
        {
            public readonly Proc Proc;
            public BasicBlock CurrentBasicBlock { get; private set; }
            public readonly Dictionary<object, Value.Register> Locals = new Dictionary<object, Value.Register>();
            public int RegisterCount { get; set; } = 0;

            public ProcContext(Proc proc)
            {
                Proc = proc;
                Debug.Assert(proc.BasicBlocks.Count == 0);
                CurrentBasicBlock = new BasicBlock();
                Proc.BasicBlocks.Add(CurrentBasicBlock);
            }

            public void SetCurrentBasicBlock(BasicBlock bb)
            {
                if (!Proc.BasicBlocks.Contains(bb))
                {
                    Proc.BasicBlocks.Add(bb);
                }
                CurrentBasicBlock = bb;
            }
        }

        /// <summary>
        /// The <see cref="Assembly"/> that holds the compiled IR code.
        /// </summary>
        public readonly Assembly Assembly;

        /// <summary>
        /// The gobals defined while compiling.
        /// </summary>
        public IReadOnlyDictionary<object, Value> Globals => globals;

        /// <summary>
        /// The registers allocated in the current procedure.
        /// </summary>
        public IReadOnlyDictionary<object, Value.Register> Locals => contextStack.Peek().Locals;

        /// <summary>
        /// The currently compiled <see cref="Proc"/>.
        /// </summary>
        public Proc CurrentProc => contextStack.Peek().Proc;

        /// <summary>
        /// The current <see cref="BasicBlock"/> that receives the compiled <see cref="Instruction"/>s.
        /// </summary>
        public BasicBlock CurrentBasicBlock
        {
            get => contextStack.Peek().CurrentBasicBlock;
            set => contextStack.Peek().SetCurrentBasicBlock(value);
        }

        // Internal state

        private Dictionary<object, Value> globals = new Dictionary<object, Value>();
        private Stack<ProcContext> contextStack = new Stack<ProcContext>();

        /// <summary>
        /// Initializes a new <see cref="IrBuilder"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to compile the IR into.</param>
        public IrBuilder(Assembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Creates a global external <see cref="Value"/>.
        /// </summary>
        /// <param name="linkName">The link name of the external.</param>
        /// <param name="type">The type of the external.</param>
        /// <param name="key">The key to associate the external's value with.</param>
        /// <returns>The created external <see cref="Value"/>.</returns>
        public Value CreateExtern(string linkName, Type type, object? key)
        {
            var value = new Value.Extern(type, linkName);
            Assembly.Externals.Add(value);
            if (key != null) globals.Add(key, value);
            return value;
        }

        /// <summary>
        /// Defines a new procedure and makes it the currently compiled procedure.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the procedure.</param>
        /// <param name="key">The key to assiciate the procedure with.</param>
        /// <returns>The created <see cref="Proc"/>.</returns>
        public Proc CreateProcBegin(Type type, object? key)
        {
            var proc = new Proc(type);
            Assembly.Procedures.Add(proc);
            if (key != null) globals.Add(key, proc);
            contextStack.Push(new ProcContext(proc));
            return proc;
        }

        /// <summary>
        /// Finishes the compilation of the current procedure, making the previous one the current again.
        /// </summary>
        public void CreateProcEnd()
        {
            contextStack.Pop();
        }

        /// <summary>
        /// Allocates a new <see cref="Value.Register"/> for the current procedure.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the value the register needs to hold.</param>
        /// <param name="key">The key the register will be associated with.</param>
        /// <returns>The newly created <see cref="Value.Register"/>.</returns>
        public Value.Register AllocateRegister(Type type, object? key)
        {
            var currentContext = contextStack.Peek();
            var reg = new Value.Register(type, currentContext.RegisterCount++);
            if (key != null) currentContext.Locals.Add(key, reg);
            return reg;
        }

        /// <summary>
        /// Same as <see cref="AllocateRegister"/>, but also adds the allocated <see cref="Value.Register"/> to the
        /// parameters of the currently compiled procedure.
        /// </summary>
        public Value.Register AllocateParameter(Type type, object? key)
        {
            var reg = AllocateRegister(type, key);
            CurrentProc.Parameters.Add(reg);
            return reg;
        }

        /// <summary>
        /// Creates a new <see cref="BasicBlock"/> for the currently compiled procedure, making the currently targeted
        /// <see cref="BasicBlock"/> this new one.
        /// </summary>
        /// <returns>The newly created <see cref="BasicBlock"/>.</returns>
        public BasicBlock CreateBasicBlock()
        {
            var bb = new BasicBlock();
            CurrentBasicBlock = bb;
            return bb;
        }

        /// <summary>
        /// Adds an <see cref="Instruction"/> to the current <see cref="BasicBlock"/>.
        /// </summary>
        /// <param name="instruction">The <see cref="Instruction"/> to add.</param>
        public void AddInstruction(Instruction instruction)
        {
            CurrentBasicBlock.Instructions.Add(instruction);
        }
    }
}
