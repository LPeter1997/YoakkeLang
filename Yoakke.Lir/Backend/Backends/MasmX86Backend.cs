﻿using System;
using System.Linq;
using System.Text;
using Yoakke.Lir.Backend.Backends.X86Family;

namespace Yoakke.Lir.Backend.Backends
{
    /// <summary>
    /// Backend for MASM for Windows on X86.
    /// </summary>
    public class MasmX86Backend : IBackend
    {
        public TargetTriplet TargetTriplet { get; set; }

        private StringBuilder code = new StringBuilder();
        private X86FormatOptions formatOptions = new X86FormatOptions
        {
            SpecialSeparator = '@',
            AllUpperCase = false,
        };

        public bool IsSupported(TargetTriplet targetTriplet) =>
               targetTriplet.CpuFamily == CpuFamily.X86
            && targetTriplet.OperatingSystem == OperatingSystem.Windows;

        public string Compile(Assembly assembly)
        {
            code.Clear();
            code.AppendLine(".386")
                .AppendLine(".MODEL flat");
            var x86asm = X86Assembler.Assemble(assembly);
            CompileAssembly(x86asm);
            code.AppendLine("END");
            return code.ToString();
        }

        private void CompileAssembly(X86Assembly assembly)
        {
            // TODO: externals
            // Compile global declarations
            code.AppendLine("_BSS SEGMENT");
            foreach (var g in assembly.Globals) DeclareGlobal(g);
            code.AppendLine("_BSS ENDS");
            // Compile procedures
            code.AppendLine(".CODE");
            foreach (var p in assembly.Procedures) CompileProc(p);
        }

        private void DeclareGlobal(X86Global global)
        {
            code
                .Append($"{global.Name} DB ")
                .AppendJoin(", ", Enumerable.Repeat("0", global.Size))
                .AppendLine(" DUP (?)");
        }

        private void CompileProc(X86Proc proc)
        {
            var visibility = proc.Visibility == Visibility.Public ? "PUBLIC" : string.Empty;
            code.AppendLine($"{proc.Name} PROC {visibility}");
            // Compile every basic block
            foreach (var bb in proc.BasicBlocks) CompileBasicBlock(bb);
            code.AppendLine($"{proc.Name} ENDP");
        }

        private void CompileBasicBlock(X86BasicBlock basicBlock)
        {
            if (basicBlock.Name != null)
            {
                code.Append(basicBlock.ToIntelSyntax(formatOptions)).AppendLine(":");
            }
            // Write out instructions
            code.AppendJoin(
                string.Empty, 
                basicBlock.Instructions.Select(ins => $"    {ins.ToIntelSyntax(formatOptions)}\n"));
        }
    }
}
