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

        public string Compile(Assembly assembly)
        {
            code.Clear();
            code.AppendLine(".386")
                .AppendLine(".MODEL flat")
                .AppendLine(".CODE");
            var x86asm = X86Assembler.Assemble(assembly);
            CompileAssembly(x86asm);
            code.AppendLine("END");
            return code.ToString();
        }

        private void CompileAssembly(X86Assembly assembly)
        {
            // TODO: externals, globals, ...
            // Compile procedures
            foreach (var p in assembly.Procedures) CompileProc(p);
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
                code.Append(basicBlock.Name.Replace('.', '@')).AppendLine(":");
            }
            // Write out instructions
            code.AppendJoin(string.Empty, basicBlock.Instructions.Select(ins => $"    {ins.ToIntelSyntax()}\n"));
        }
    }
}
