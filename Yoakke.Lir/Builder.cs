using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// Utility for building IR code in an <see cref="Assembly"/>.
    /// </summary>
    public class Builder
    {
        private class ProcContext
        {
            public int RegisterCount { get; set; }
        }

        /// <summary>
        /// The <see cref="Assembly"/> the <see cref="Builder"/> works with.
        /// </summary>
        public readonly Assembly Assembly;

        /// <summary>
        /// The currently built <see cref="Proc"/>.
        /// </summary>
        public Proc CurrentProc 
        { 
            get
            {
                // TODO
                if (currentProc == null) throw new NotImplementedException();
                return currentProc;
            }
            set => currentProc = value;
        }
        /// <summary>
        /// The currently built <see cref="BasicBlock"/>.
        /// </summary>
        public BasicBlock CurrentBasicBlock
        {
            get
            {
                // TODO
                if (currentBasicBlock == null) throw new NotImplementedException();
                return currentBasicBlock;
            }
            set => currentBasicBlock = value;
        }

        private Proc? currentProc;
        private BasicBlock? currentBasicBlock;
        private IDictionary<Proc, ProcContext> procContexts = new Dictionary<Proc, ProcContext>();

        /// <summary>
        /// Initializes a new <see cref="Builder"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to build IR code in.</param>
        public Builder(Assembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Adds a new <see cref="Extern"/> symbol definition to the <see cref="Assembly"/>.
        /// </summary>
        /// <param name="name">The name of the external symbol.</param>
        /// <param name="type">The <see cref="Type"/> of the external symbol.</param>
        /// <param name="path">The path to the binary containing the external symbol.</param>
        /// <returns>The <see cref="Value"/> referring to the external symbol.</returns>
        public Value DefineExtern(string name, Type type, string path)
        {
            // TODO: Check name uniqueness
            var external = new Extern(name, type, path);
            Assembly.Externals.Add(external);
            return external;
        }

        // TODO: Return value instead
        public Proc DefineProc(string name)
        {
            // TODO: Check name uniqueness
            var proc = new Proc(name);
            CurrentProc = proc;
            Assembly.Procedures.Add(proc);
            DefineBasicBlock("begin");
            return proc;
        }

        // TODO: Doc
        // TODO: Allocate and assign implicitly?
        public Value DefineParameter(Type type)
        {
            var reg = AllocateRegister(type);
            CurrentProc.Parameters.Add(reg);
            return reg;
        }

        // TODO: Doc
        public BasicBlock DefineBasicBlock(string name)
        {
            var bb = new BasicBlock(name);
            CurrentBasicBlock = bb;
            CurrentProc.BasicBlocks.Add(bb);
            return bb;
        }

        // Instructions ////////////////////////////////////////////////////////

        // TODO: Doc
        public void AddInstruction(Instr instr) =>
            CurrentBasicBlock.Instructions.Add(instr);

        // TODO: Doc
        public void Ret(Value value) => AddInstruction(new Instr.Ret(value));

        // TODO: Doc
        public Value Call(Value procedure, IList<Value> arguments)
        {
            if (!(procedure.Type is Type.Proc procType))
            {
                throw new ArgumentException("The procedure value must have a procedure type!", nameof(procedure));
            }
            if (!procType.Parameters.SequenceEqual(arguments.Select(arg => arg.Type)))
            {
                throw new ArgumentException("The procedure is not callable with the given arguments!", nameof(arguments));
            }
            var resultReg = AllocateRegister(procType.Return);
            AddInstruction(new Instr.Call(resultReg, procedure, arguments));
            return resultReg;
        }

        // Internals

        private Register AllocateRegister(Type type)
        {
            if (!procContexts.TryGetValue(CurrentProc, out var ctx))
            {
                ctx = new ProcContext();
                procContexts.Add(CurrentProc, ctx);
            }
            return new Register(type, ctx.RegisterCount++);
        }
    }
}
