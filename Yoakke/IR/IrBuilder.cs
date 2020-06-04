using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Semantic;
using Yoakke.Utils;

namespace Yoakke.IR
{
    class IrBuilder
    {
        public readonly Assembly Assembly;

        public Proc CurrentProc
        {
            get { Assert.NonNull(currentProc); return currentProc; }
        }

        public BasicBlock CurrentBasicBlock
        {
            get { Assert.NonNull(currentBB); return currentBB; }
        }

        private Dictionary<ProcValue, Proc> compiledProcedures = new Dictionary<ProcValue, Proc>();
        private HashSet<string> globalNames = new HashSet<string>();
        private Proc? currentProc;
        private BasicBlock? currentBB;
        private HashSet<string>? currentLocalNames = null;

        public IrBuilder(Assembly assembly)
        {
            Assembly = assembly;
        }

        public bool TryGetProc(ProcValue procValue, out Proc? proc) =>
            compiledProcedures.TryGetValue(procValue, out proc);

        public void CreateProc(string name, Type returnType, Action action)
        {
            name = GlobalUniqueName(name);

            var lastProc = currentProc;
            var lastBB = currentBB;
            var lastLocalNames = currentLocalNames;

            currentProc = new Proc(name, returnType);
            currentLocalNames = new HashSet<string>();
            CreateBasicBlock("begin");
            action();
            
            currentProc = lastProc;
            currentBB = lastBB;
            currentLocalNames = lastLocalNames;
        }

        public void CreateBasicBlock(string name)
        {
            Assert.NonNull(currentProc);

            name = LocalUniqueName(name);
            currentBB = new BasicBlock(name);
            currentProc.BasicBlocks.Add(currentBB);
        }

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
