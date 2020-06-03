using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    class Assembly
    {
        public readonly List<Proc> Procedures = new List<Proc>();
    }

    class Proc
    {
        public readonly string Name;
        public readonly Type ReturnType;
        public readonly List<BasicBlock> BasicBlocks = new List<BasicBlock>();

        public Proc(string name, Type returnType)
        {
            Name = name;
            ReturnType = returnType;
        }
    }

    class BasicBlock
    {
        public readonly string Name;
        public readonly List<Instruction> Instructions = new List<Instruction>();

        public BasicBlock(string name)
        {
            Name = name;
        }
    }
}
