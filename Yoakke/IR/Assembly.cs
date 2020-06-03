using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.IR
{
    class Assembly
    {
        public List<Proc> Procedures { get; } = new List<Proc>();
    }

    class Proc
    {
        public readonly string Name;
        public readonly Type ReturnType;
        public List<BasicBlock> BasicBlocks { get; } = new List<BasicBlock>();

        public Proc(string name, Type returnType)
        {
            Name = name;
            ReturnType = returnType;
        }
    }

    class BasicBlock
    {
        public readonly string Name;

        public BasicBlock(string name)
        {
            Name = name;
        }
    }
}
