using System;
using System.Linq;
using System.Text;
using Yoakke.Lir.Instructions;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Backend.Backends
{
    /// <summary>
    /// Backend for MASM for Windows on X86.
    /// </summary>
    public class MasmX86Backend : IBackend
    {
        public TargetTriplet TargetTriplet { get; set; }

        private StringBuilder globalsCode = new StringBuilder();
        private StringBuilder textCode = new StringBuilder();

        public string Compile(Assembly assembly)
        {
            globalsCode.Clear();
            textCode.Clear();
            CompileAssembly(assembly);

            // Stitch code together
            return new StringBuilder()
                .AppendLine(".386")
                .AppendLine(".MODEL flat")
                .Append(globalsCode)
                .AppendLine(".CODE")
                .Append(textCode)
                .AppendLine("END")
                .ToString();
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
            globalsCode.AppendLine($"EXTERN {GetExternName(ext)}: {TypeToString(ext.Type)}");
        }

        private void CompileProc(Proc proc)
        {
            var procName = GetSymbolName(proc);
            var visibility = proc.Visibility == Visibility.Public ? "PUBLIC" : string.Empty;
            textCode.AppendLine($"{procName} PROC {visibility}");
            // Just compile every basic block
            foreach (var bb in proc.BasicBlocks) CompileBasicBlock(proc, bb);
            textCode.AppendLine($"{procName} ENDP");
        }

        private void CompileBasicBlock(Proc proc, BasicBlock basicBlock)
        {
            bool first = ReferenceEquals(proc.BasicBlocks[0], basicBlock);

            if (proc.CallConv != CallConv.Cdecl) throw new NotImplementedException();

            // Now just write the label name, then the instructions
            if (!first) textCode.AppendLine($"{GetSymbolName(proc)}@{basicBlock.Name}:");
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
            Value.Symbol e => $"[{GetSymbolName(e.Value)}]",
            _ => throw new NotImplementedException(),
        };

        private string GetSymbolName(ISymbol symbol) =>
               // For Cdecl procedures we assume an underscore prefix
               (symbol is Proc proc && proc.CallConv == CallConv.Cdecl)
            // For non-procedures too
            || !(symbol is Proc)
            ? $"_{symbol.Name}" : symbol.Name;

        private string GetExternName(Extern ext) =>
            // NOTE: We need a '_' prefix here too
            TargetTriplet.OperatingSystem == OperatingSystem.Windows
            ? $"_{ext.Name}" : ext.Name;

        private string TypeToString(Type type) => type switch
        {
            Type.Int i => ((i.Bits + 7) / 8) switch
            {
                1 => "BYTE",
                2 => "WORD",
                4 => "DWORD",
                _ => throw new NotImplementedException(),
            },
            _ => throw new NotImplementedException(),
        };

        private int SizeOf(Value value) => SizeOf(value.Type);
        private int SizeOf(Type type) => type switch
        {
            Type.Int i => (i.Bits + 7) / 8,
            _ => throw new NotImplementedException(),
        };
    }
}
