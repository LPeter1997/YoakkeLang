using System;
using System.Collections.Generic;
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
        private IDictionary<Register, int> registerOffsets = new Dictionary<Register, int>();

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
            globalsCode.AppendLine($"EXTERN {GetSymbolName(ext)}: {TypeToString(ext.Type)}");
        }

        private void CompileProc(Proc proc)
        {
            var procName = GetSymbolName(proc);
            var visibility = proc.Visibility == Visibility.Public ? "PUBLIC" : string.Empty;
            textCode.AppendLine($"{procName} PROC {visibility}");

            // Save stack base pointer, set it to the current stack pointer
            textCode.AppendLine("    push ebp");
            textCode.AppendLine("    mov ebp, esp");
            // Find how big of an allocation we need, assign each register to it's offset
            registerOffsets.Clear();
            var registers = proc.BasicBlocks
                .SelectMany(bb => bb.Instructions)
                .Where(ins => ins is ValueInstr)
                .Cast<ValueInstr>()
                .Select(ins => ins.Result);
            var allocSize = registers.Select(r => SizeOf(r.Type)).Sum();
            // Parameters
            // Start from 4 because of the top one being the return address
            int offset = 4;
            foreach (var r in proc.Parameters)
            {
                offset += SizeOf(r.Type);
                registerOffsets[r] = offset;
            }
            // Locals
            offset = 0;
            foreach (var r in registers)
            {
                offset -= SizeOf(r.Type);
                registerOffsets[r] = offset;
            }
            // Allocate that on the stack
            textCode.AppendLine($"    sub esp, {allocSize}");
            // Compile every basic block
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
                // Return value
                if (valueSize > 0)
                {
                    // Store return value
                    if (proc.CallConv == CallConv.Cdecl && ret.Value.Type is Type.Int && SizeOf(ret.Value) <= 4)
                    {
                        // We can return integral values with at most 32 bits in EAX
                        textCode.AppendLine($"    mov eax, {CompileValue(ret.Value, false)}");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                // Reset stack base pointer, return
                textCode.AppendLine("    mov esp, ebp");
                textCode.AppendLine("    pop ebp");
                textCode.AppendLine("    ret");
            } 
            break;

            case Instr.Call call:
            {
                // TODO: Proper error
                if (!(call.Procedure.Type is Type.Proc procTy))
                {
                    throw new InvalidOperationException();
                }
                if (procTy.CallConv == CallConv.Cdecl)
                {
                    // Push arguments backwards, track the offset of esp
                    var espOffset = 0;
                    foreach (var arg in call.Arguments.Reverse())
                    {
                        textCode.AppendLine($"    push {CompileValue(arg, false)}");
                        espOffset += SizeOf(arg.Type);
                    }
                    // Do the call
                    // TODO: Do we want an lvalue here?
                    // What if it's a pointer by value?
                    textCode.AppendLine($"    call {CompileValue(call.Procedure, true)}");
                    // Restore stack
                    textCode.AppendLine($"    add esp, {espOffset}");
                    // TODO: Only if size is fine
                    // Store value
                    textCode.AppendLine($"    mov {CompileValue(new Value.Register(call.Result), false)}, eax");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            break;

            default: throw new NotImplementedException();
            }
        }

        private string CompileValue(Value value, bool lvalue)
        {
            switch (value)
            {
            case Value.Int i:
                // TODO: Proper error
                if (lvalue) throw new InvalidOperationException();
                return i.Value.ToString();

            case Value.Symbol sym:
            {
                var symName = GetSymbolName(sym.Value);
                return lvalue ? symName : $"[{symName}]";
            }

            case Value.Register reg:
            {
                var offset = registerOffsets[reg.Value];
                var address = offset > 0 ? $"ebp + {offset}" : $"ebp - {-offset}";
                return lvalue ? address : $"[{address}]";
            }

            default: throw new NotImplementedException();
            }
        }

        private string GetSymbolName(ISymbol symbol) =>
               // For Cdecl procedures we assume an underscore prefix
               (symbol is Proc proc && proc.CallConv == CallConv.Cdecl)
            // For non-procedures too
            || !(symbol is Proc)
            ? $"_{symbol.Name}" : symbol.Name;

        private string TypeToString(Type type) => type switch
        {
            Type.Int _ => SizeOf(type) switch
            {
                1 => "BYTE",
                2 => "WORD",
                4 => "DWORD",
                _ => throw new NotImplementedException(),
            },
            _ => throw new NotImplementedException(),
        };

        private static int SizeOf(Value value) => SizeOf(value.Type);
        private static int SizeOf(Type type) => type switch
        {
            // First we round up to bytes, then make sure it's a power of 2
            Type.Int i => NextPow2((i.Bits + 7) / 8),
            _ => throw new NotImplementedException(),
        };

        private static int NextPow2(int n)
        {
            int result = 1;
            while (result < n) result = result << 1;
            return result;
        }
    }
}
