using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Syntax
{
    /// <summary>
    /// The parse tree to AST conversion.
    /// </summary>
    public static class ParseTreeToAst
    {
        /// <summary>
        /// Converts a parse-tree statement to an AST statement.
        /// </summary>
        /// <param name="s">The parse-tree statement.</param>
        /// <returns>The equivalend AST statement.</returns>
        public static Ast.Statement Convert(ParseTree.Statement stmt) => stmt switch
        {
            ParseTree.Declaration.File file => 
                new Ast.Declaration.File(file, file.Declarations.Select(Convert).ToArray()),

            ParseTree.Declaration.Definition def => def.Keyword.Type == TokenType.KwConst
                ? new Ast.Declaration.Const(
                    def, 
                    def.Name.Value, 
                    ConvertNullable(def.Type), 
                    Convert(def.Value ?? throw new NotImplementedException())
                    )
                : new Ast.Statement.Var(
                    def,
                    def.Name.Value,
                    ConvertNullable(def.Type),
                    ConvertNullable(def.Value)),

            ParseTree.Statement.Expression_ expr =>
                new Ast.Statement.Expression_(expr, Convert(expr.Expression)),

            ParseTree.Statement.Return ret =>
                new Ast.Statement.Return(ret, ConvertNullable(ret.Value)),

            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Converts a parse-tree expression to an AST expression.
        /// </summary>
        /// <param name="s">The parse-tree expression.</param>
        /// <returns>The equivalend AST expression.</returns>
        public static Ast.Expression Convert(ParseTree.Expression expr) => expr switch
        {
            ParseTree.Expression.Literal lit => lit.Token.Type == TokenType.Identifier
                ? new Ast.Expression.Identifier(lit, lit.Token.Value)
                : new Ast.Expression.Literal(lit, lit.Token.Type, lit.Token.Value),

            ParseTree.Expression.StructType sty => 
                new Ast.Expression.StructType(sty, sty.Fields.Select(Convert).ToArray()),

            ParseTree.Expression.StructValue sval =>
                new Ast.Expression.StructValue(sval, Convert(sval.Type), sval.Fields.Select(Convert).ToArray()),

            ParseTree.Expression.ProcSignature sign =>
                new Ast.Expression.ProcSignature(
                    sign, 
                    sign.Parameters.Select(p => p.Element).Select(Convert).ToArray(), 
                    ConvertNullable(sign.Return)),

            ParseTree.Expression.Proc proc =>
                new Ast.Expression.Proc(proc, (Ast.Expression.ProcSignature)Convert(proc.Signature), Convert(proc.Body)),

            ParseTree.Expression.Block block =>
                new Ast.Expression.Block(block, block.Statements.Select(Convert).ToArray(), ConvertNullable(block.Value)),

            ParseTree.Expression.Call call =>
                new Ast.Expression.Call(
                    call, 
                    Convert(call.Procedure), 
                    call.Arguments.Select(a => a.Element).Select(Convert).ToArray()),

            ParseTree.Expression.If iff => ConvertIf(iff),

            ParseTree.Expression.While whil =>
                new Ast.Expression.While(whil, Convert(whil.Condition), Convert(whil.Body)),

            ParseTree.Expression.Binary bin =>
                new Ast.Expression.Binary(bin, Convert(bin.Left), bin.Operator.Type, Convert(bin.Right)),

            ParseTree.Expression.DotPath dot =>
                new Ast.Expression.DotPath(dot, Convert(dot.Left), dot.Right.Value),

            ParseTree.Expression.Parenthesized paren => Convert(paren.Inside),

            _ => throw new NotImplementedException(),
        };

        public static Ast.Expression.StructType.Field Convert(ParseTree.Expression.StructType.Field f) =>
            new Ast.Expression.StructType.Field(f, f.Name.Value, Convert(f.Type));

        public static Ast.Expression.StructValue.Field Convert(ParseTree.Expression.StructValue.Field f) =>
            new Ast.Expression.StructValue.Field(f, f.Name.Value, Convert(f.Value));

        public static Ast.Expression.ProcSignature.Parameter Convert(ParseTree.Expression.ProcSignature.Parameter p) =>
            new Ast.Expression.ProcSignature.Parameter(p, p.Name?.Value, Convert(p.Type));

        private static Ast.Expression? ConvertNullable(ParseTree.Expression? expr) =>
            expr == null ? null : Convert(expr);

        private static Ast.Expression ConvertIf(ParseTree.Expression.If iff)
        {
            var cond = Convert(iff.Condition);
            var then = Convert(iff.Then);
            Ast.Expression? els = null;
            if (iff.Else != null)
            {
                els = Convert(iff.Else);
            }
            foreach (var elif in iff.ElseIfs.Reverse())
            {
                els = new Ast.Expression.If(elif, Convert(elif.Condition), Convert(elif.Then), els);
            }
            return new Ast.Expression.If(iff, cond, then, els);
        }
    }
}
