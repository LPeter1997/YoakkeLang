﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend.Toolchain.Msvc
{
    /// <summary>
    /// The MSVC MASM assembler.
    /// </summary>
    public class MsvcAssembler : MsvcToolBase, IAssembler
    {
        public TargetTriplet TargetTriplet { get; set; }
        public IList<string> SourceFiles { get; } = new List<string>();
        public OutputKind OutputKind { get; set; } = OutputKind.Object;

        public MsvcAssembler(string vcVarsAllPath) 
            : base(vcVarsAllPath)
        {
        }

        public int Execute(string outputPath)
        {
            throw new NotImplementedException();
        }
    }
}
