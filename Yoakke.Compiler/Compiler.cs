using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;
using Yoakke.Compiler.IR;
using Yoakke.Compiler.IR.Passes;
using Yoakke.Compiler.Codegen;
using Yoakke.Compiler.Semantic;
using System.Runtime.Serialization;
using System.IO;
using System.Diagnostics;

namespace Yoakke.Compiler
{
    public enum Backend
    {
        C,
    }

    /*
     TODO: Consider moving every builtin into compiler intrinsics, exposing the IR.
     Example:

     const i32 = @ir.integer(true, 32); // Signed, 32 bits
     const `+` = proc(x: i32, y: i32) -> i32 { @ir.iadd(x, y) };
     */

    public class Compiler
    {
        // Input

        public Source? Source { get; set; }

        [Required(ErrorMessage = "A file is required to compile!")]
        [Argument(0, Name = "file", Description = "The file to compile")]
        public string? SourceFile { get; set; }

        // Debug intercept

        [Option(LongName = "dump-tokens", ShortName = "", Description = "Dump the lexed tokens code instead of compiling")]
        public bool DumpTokens { get; set; }

        [Option(LongName = "dump-ast", ShortName = "", Description = "Dump the AST instead of compiling")]
        public bool DumpAst { get; set; }

        [Option(LongName = "dump-ir", ShortName = "", Description = "Dump the IR code instead of compiling")]
        public bool DumpIr { get; set; }

        [Option(LongName = "dump-backend", ShortName = "", Description = "Dump the backend code instead of compiling")]
        public bool DumpBackend { get; set; }

        // Debug helper

        [Option(LongName = "execute", ShortName = "e", Description = "Execute the produced program right after compiling")]
        public bool ExecuteImmediately { get; set; }

        // Compile options

        [Option(ShortName = "o", LongName = "output", Description = "The output path")]
        public string OutputPath { get; set; } = "a.out";

        [Option(ShortName = "O", LongName = "", Description = "The optimization level")]
        public int OptimizationLevel { get; set; } = 0;

        [Option(ShortName = "ot", LongName = "output-type", Description = "The output type")]
        public OutputType OutputType { get; set; } = OutputType.Exe;

        // Compiler backend

        [Option(LongName = "backend", Description = "The backend to use", ValueName = "backend")]
        public Backend Backend { get; set; } = Backend.C;

        [Option(LongName = "ccompiler", ShortName = "cc", Description = "The C compiler to use", ValueName = "compiler")]
        public string CCompiler { get; set; } = "gcc";

        public void OnExecute()
        {
            try
            {
                int exitCode = Execute();
                Environment.Exit(exitCode);
            }
            catch (CompileError err)
            {
                err.Show();
                Environment.Exit(1);
            }
        }

        public int Execute()
        {
            // Read in the source
            if (Source == null)
            {
                Assert.NonNull(SourceFile);
                if (!File.Exists(SourceFile))
                {
                    Console.WriteLine($"Input file '{SourceFile}' not found!");
                    return 1;
                }
                Source = new Source(SourceFile, File.ReadAllText(SourceFile));
            }

            // Tokenize
            var tokens = Lexer.Lex(Source.Value);

            // If we want to dump tokens, do it now
            if (DumpTokens)
            {
                foreach (var t in tokens) Console.WriteLine($"'{t.Value}' - {t.Type} ({t.Position})");
                return 0;
            }

            // Parse
            var ast = Parser.ParseProgram(tokens);

            // If we want to dump the AST, do it now
            if (DumpAst)
            {
                Console.WriteLine(ast.DumpTree());
                return 0;
            }

            // Do semantic checks
            Checks.CheckAll(ast);

            // Compile to IR
            var asm = IR.Compiler.Compile(ast);

            // Optimize it
            var passes = PassesForOptimizationLevel();
            Optimizer.Optimize(asm, passes);

            var namingCtx = new NamingContext(asm);

            // If we want to dump IR, do it here
            if (DumpIr)
            {
                Console.WriteLine(IrDump.Dump(namingCtx));
                return 0;
            }

            // If the output type is IR, just write that out now
            if (OutputType == OutputType.IR)
            {
                var ir = IrDump.Dump(namingCtx);
                File.WriteAllText(SourceFile, ir);
                return 0;
            }

            // Get the proper backend
            var backend = GetBackend();

            // If we want to dump backend code, do it here
            if (DumpBackend)
            {
                var backendCode = backend.Compile(namingCtx);
                Console.WriteLine(backendCode);
                return 0;
            }

            // Otherwise let's just produce the output
            var exitCode = backend.CompileAndOutput(namingCtx, OutputPath, OutputType, new object[] { });
            if (exitCode != 0) return exitCode;

            // If we need to execute it, do it now
            if (ExecuteImmediately)
            {
                Console.WriteLine($"Running '{OutputPath}'...");

                var startInfo = new ProcessStartInfo(OutputPath);
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;

                var process = Process.Start(startInfo);
                while (!process.StandardError.EndOfStream)
                {
                    string? line = process.StandardError.ReadLine();
                    if (line != null) Console.WriteLine(line);
                }
                process.WaitForExit();

                Console.WriteLine($"Exit code: {process.ExitCode}");
                Console.Out.Flush();
                return process.ExitCode;
            }

            return 0;
        }

        private List<IPass> PassesForOptimizationLevel()
        {
            var passes = new List<IPass> { new RemoveVoid() };
            if (OptimizationLevel > 0)
            {
                // Add level 1 optimizations
                passes.Add(new JumpThreading());
                passes.Add(new DeadCodeElimination());
                passes.Add(new ConstantFolding());
                passes.Add(new BlockMerging());
            }
            return passes;
        }

        private ICodegen GetBackend()
        {
            switch (Backend)
            {
            case Backend.C: return new CCodegen(CCompiler);

            default: throw new NotImplementedException();
            }
        }
    }
}
