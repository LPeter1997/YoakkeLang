using System;
using System.Linq;
using System.Text;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Backend.Backends
{
    /// <summary>
    /// Backend for NASM on X86.
    /// </summary>
    public class NasmX86Backend : IBackend
    {
        public Toolchain Toolchain { get; set; }

        private TargetTriplet targetTriplet;
        private StringBuilder globalsCode = new StringBuilder();
        private StringBuilder textCode = new StringBuilder();

        // Pointer size in bytes
        private int PointerSize => targetTriplet.CpuFamily switch
        {
            CpuFamily.X86 => 4,
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Initializes a new <see cref="NasmX86Backend"/>.
        /// </summary>
        /// <param name="toolchain">The <see cref="Toolchain"/> to be used by the NASM backend.</param>
        public NasmX86Backend(Toolchain toolchain)
        {
            Toolchain = toolchain;
        }

        public bool IsSupported(TargetTriplet t) =>
            t.CpuFamily == CpuFamily.X86 && t.OperatingSystem == OperatingSystem.Windows;

        public string Compile(TargetTriplet targetTriplet, Assembly assembly)
        {
            if (!IsSupported(targetTriplet))
            {
                throw new NotSupportedException("The given target triplet is not supported by this backend!");
            }
            this.targetTriplet = targetTriplet;

            globalsCode.Clear();
            textCode.Clear();
            CompileAssembly(assembly);

            // Stitch code together
            return new StringBuilder()
                .AppendLine($"[BITS {PointerSize * 8}]")
                .Append(globalsCode)
                .AppendLine("SECTION .TEXT")
                .Append(textCode)
                .ToString()
                .Trim();
        }

        private void CompileAssembly(Assembly assembly)
        {
            // Add externals
            foreach (var e in assembly.Externals) CompileExtern(e);
            // Compile procedures
            foreach (var p in assembly.Procedures) CompileProc(p);
        }

        private void CompileExtern(Extern ext)
        {
            // TODO: Should calling convention affect name in case of external procedures?
            globalsCode.AppendLine($"EXTERN {GetExternName(ext)}");
        }

        private void CompileProc(Proc proc)
        {
            // Just compile every basic block
            foreach (var bb in proc.BasicBlocks) CompileBasicBlock(proc, bb);
        }

        private void CompileBasicBlock(Proc proc, BasicBlock basicBlock)
        {
            bool first = ReferenceEquals(proc.BasicBlocks[0], basicBlock);

            if (proc.CallConv != CallConv.Cdecl) throw new NotImplementedException();

            // If this is the first basic block, we use the procedure's name as the label name
            // Otherwise we allocate an unused label name
            // TODO: This is not a bulletproof name allocation
            var procName = GetProcName(proc);
            var labelName = first ? procName : $"{procName}.{basicBlock.Name}";
            // If it's a public procedure, we need to define it global
            if (first && proc.Visibility == Visibility.Public) globalsCode.AppendLine($"GLOBAL {labelName}");

            // Now just write the label name, then the instructions
            textCode.AppendLine($"{labelName}:");
            // Write out instructions
            foreach (var ins in basicBlock.Instructions) CompileInstruction(proc, ins);
        }

        private void CompileInstruction(Proc proc, Instr instr)
        {
            switch (instr)
            {
            case Instr.Ret ret:
            {
                var valueSize = SizeOf(ret.Value);
                if (valueSize > 0)
                {
                    // Store return value
                    if (proc.CallConv == CallConv.Cdecl && ret.Value.Type is Type.Int && SizeOf(ret.Value) <= 4)
                    {
                        // We can return integral values with at most 32 bits in EAX
                        textCode.AppendLine($"    mov eax, {CompileValue(ret.Value)}");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                textCode.AppendLine("    ret");
            } 
            break;

            default: throw new NotImplementedException();
            }
        }

        private string CompileValue(Value value) => value switch
        {
            // TODO: Lvalue vs rvalue?
            Value.Int i => i.ToString(),
            Value.Extern e => $"[{GetExternName(e.Value)}]",
            _ => throw new NotImplementedException(),
        };

        private string GetProcName(Proc proc) =>
               // On Windows, Cdecl will cause a '_' prefix
               targetTriplet.OperatingSystem == OperatingSystem.Windows
            && proc.CallConv == CallConv.Cdecl
               ? $"_{proc.Name}" : proc.Name;

        private string GetExternName(Extern ext) =>
            // NOTE: We need a '_' prefix here too
            targetTriplet.OperatingSystem == OperatingSystem.Windows
            ? $"_{ext.Name}" : ext.Name;

        private int SizeOf(Value value) => SizeOf(value.Type);
        private int SizeOf(Type type) => type switch
        {
            Type.Int i => (i.Bits + 7) / 8,
            _ => throw new NotImplementedException(),
        };
    }
}
