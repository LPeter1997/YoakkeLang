using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Semantic.Types;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Types.Type;

namespace Yoakke.Compiler.Compile
{
    // TODO: Doc
    public class CollectDependencies : Visitor<object>
    {
        public IDependencySystem System { get; }

        public readonly IDictionary<Expression.Proc, Expression.Proc> ProcDesugar 
            = new Dictionary<Expression.Proc, Expression.Proc>();
        public readonly IDictionary<Expression.Call, Expression.Call> CallDesugar
            = new Dictionary<Expression.Call, Expression.Call>();

        public CollectDependencies(IDependencySystem system)
        {
            System = system;
        }

        public void Collect(Declaration.File file) => Visit(file);

        protected override object? Visit(Expression.Proc proc)
        {
            base.Visit(proc);

            var procType = (Type.Proc)System.TypeOf(proc);
            var dependency = procType.GetDependency();
            if (dependency != null)
            {
                /*
                Desugar
                    proc(dependee args..., dependent args..., independent args...) -> Ret {
                        body
                    }
                Into
                    proc(dependee args...) -> type {
                        struct { 
                            const f = proc(dependent args..., independent args...) -> Ret {
                                body
                            };
                        }
                    }
                */

                var result = new Expression.Proc(
                    null,
                    new Expression.ProcSignature(
                        null,
                        dependency.DependeeIndices
                            .Select(i => proc.Signature.Parameters[i])
                            .ToArray(),
                        new Expression.Identifier(null, "type")),
                    new Expression.StructType(
                        null,
                        new Syntax.Token(new Text.Span(), Syntax.TokenType.KwStruct, "struct"),
                        new Expression.StructType.Field[] { },
                        new Statement[]
                        {
                            new Declaration.Const(
                                null,
                                "f",
                                null,
                                new Expression.Proc(
                                    null,
                                    new Expression.ProcSignature(
                                        null,
                                        dependency.DependentIndices
                                            .Concat(dependency.IndependentIndices)
                                            .Select(i => proc.Signature.Parameters[i])
                                            .ToArray(),
                                        proc.Signature.Return),
                                    proc.Body))
                        }));

                ProcDesugar.Add(proc, result);
            }
            return null;
        }

        protected override object? Visit(Expression.Call call)
        {
            base.Visit(call);
            var calledType = System.TypeOf(call.Procedure);
            if (calledType is Type.Proc procType)
            {
                var dependency = procType.GetDependency();
                if (dependency != null)
                {
                    /*
                    Desugar
                        f(dependee args..., independent args..., dependent args...)
                    Into
                        f(dependee args...).f(independent args..., dependent args...)
                    */

                    var result = new Expression.Call(
                        null,
                        new Expression.DotPath(
                            null,
                            new Expression.Call(
                                null,
                                call.Procedure,
                                dependency.DependeeIndices
                                    .Select(i => call.Arguments[i])
                                    .ToArray()),
                            "f"),
                        dependency.DependentIndices
                            .Concat(dependency.IndependentIndices)
                            .Select(i => call.Arguments[i])
                            .ToArray());

                    CallDesugar.Add(call, result);
                }
            }
            return null;
        }
    }
}
