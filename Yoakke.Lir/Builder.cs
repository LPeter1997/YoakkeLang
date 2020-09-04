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
        public Value.Symbol DefineExtern(string name, Type type, string path)
        {
            // TODO: Check name uniqueness
            var external = new Extern(name, type, path);
            Assembly.Externals.Add(external);
            return new Value.Symbol(external);
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
        public BasicBlock DefineBasicBlock(string name)
        {
            var bb = new BasicBlock(name);
            CurrentBasicBlock = bb;
            CurrentProc.BasicBlocks.Add(bb);
            return bb;
        }

        // Instructions ////////////////////////////////////////////////////////

        public void Ret(Value value) => 
            CurrentBasicBlock.Instructions.Add(new Instr.Ret(value));
    }
}
