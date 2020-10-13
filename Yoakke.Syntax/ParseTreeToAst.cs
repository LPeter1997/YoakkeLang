using System;
using System.Linq;
using System.Linq.Expressions;

namespace Yoakke.Syntax
{
    /// <summary>
    /// The parse tree to AST conversion.
    /// </summary>
    public static class ParseTreeToAst
    {
        /// <summary>
        /// Converts the parse-tree file to an AST file.
        /// </summary>
        /// <param name="file">The <see cref="ParseTree.Declaration.File"/> to convert.</param>
        /// <returns>The converted <see cref="Ast.Declaration.File"/>.</returns>
        public static Ast.Declaration.File Convert(ParseTree.Declaration.File file) =>
            (Ast.Declaration.File)Convert((ParseTree.Statement)file);

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
                new Ast.Statement.Expression_(expr, Convert(expr.Expression), expr.Semicolon != null),

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
                : new Ast.Expression.Literal(lit, ToLiteralType(lit.Token.Type), lit.Token.Value),

            ParseTree.Expression.ArrayType aty =>
                new Ast.Expression.ArrayType(aty, Convert(aty.Length), Convert(aty.ElementType)),

            ParseTree.Expression.StructType sty => 
                new Ast.Expression.StructType(sty, sty.KwStruct, sty.Fields.Select(Convert).ToArray()),

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

            ParseTree.Expression.Subscript sub =>
                new Ast.Expression.Subscript(sub, Convert(sub.Array), Convert(sub.Index)),

            ParseTree.Expression.If iff => ConvertIf(iff),

            ParseTree.Expression.While whil =>
                new Ast.Expression.While(whil, Convert(whil.Condition), Convert(whil.Body)),

            ParseTree.Expression.Binary bin =>
                new Ast.Expression.Binary(
                    bin, 
                    Convert(bin.Left), 
                    ToBinaryOperator(bin.Operator.Type), 
                    Convert(bin.Right)),

            ParseTree.Expression.Prefix pre =>
                new Ast.Expression.Unary(
                    pre, 
                    ToPrefixUnaryOperator(pre.Operator.Type), 
                    Convert(pre.Operand)),

            ParseTree.Expression.Postfix post =>
                new Ast.Expression.Unary(
                    post,
                    ToPostfixUnaryOperator(post.Operator.Type),
                    Convert(post.Operand)),

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

        private static Ast.Expression.LiteralType ToLiteralType(TokenType tokenType) => tokenType switch
        {
            TokenType.IntLiteral => Ast.Expression.LiteralType.Integer,
            TokenType.KwTrue => Ast.Expression.LiteralType.Bool,
            TokenType.KwFalse => Ast.Expression.LiteralType.Bool,
            TokenType.StringLiteral => Ast.Expression.LiteralType.String,
            _ => throw new NotImplementedException(),
        };

        private static Ast.Expression.BinaryOperator ToBinaryOperator(TokenType tokenType) => tokenType switch
        {
            TokenType.Assign         => Ast.Expression.BinaryOperator.Assign        ,
            TokenType.AddAssign      => Ast.Expression.BinaryOperator.AddAssign     , 
            TokenType.SubtractAssign => Ast.Expression.BinaryOperator.SubtractAssign,
            TokenType.MultiplyAssign => Ast.Expression.BinaryOperator.MultiplyAssign, 
            TokenType.DivideAssign   => Ast.Expression.BinaryOperator.DivideAssign  , 
            TokenType.ModuloAssign   => Ast.Expression.BinaryOperator.ModuloAssign  ,
            TokenType.Or             => Ast.Expression.BinaryOperator.Or            ,
            TokenType.And            => Ast.Expression.BinaryOperator.And           ,
            TokenType.Equal          => Ast.Expression.BinaryOperator.Equals        , 
            TokenType.NotEqual       => Ast.Expression.BinaryOperator.NotEquals     ,
            TokenType.Greater        => Ast.Expression.BinaryOperator.Greater       , 
            TokenType.GreaterEqual   => Ast.Expression.BinaryOperator.GreaterEqual  , 
            TokenType.Less           => Ast.Expression.BinaryOperator.Less          , 
            TokenType.LessEqual      => Ast.Expression.BinaryOperator.LessEqual     ,
            TokenType.Add            => Ast.Expression.BinaryOperator.Add           , 
            TokenType.Subtract       => Ast.Expression.BinaryOperator.Subtract      ,
            TokenType.Multiply       => Ast.Expression.BinaryOperator.Multiply      , 
            TokenType.Divide         => Ast.Expression.BinaryOperator.Divide        , 
            TokenType.Modulo         => Ast.Expression.BinaryOperator.Modulo        ,
            _                        => throw new NotImplementedException()         ,
        };

        private static Ast.Expression.UnaryOperator ToPrefixUnaryOperator(TokenType tokenType) => tokenType switch
        {
            TokenType.Add      => Ast.Expression.UnaryOperator.Ponote     ,
            TokenType.Subtract => Ast.Expression.UnaryOperator.Negate     ,
            TokenType.Multiply => Ast.Expression.UnaryOperator.PointerType,
            TokenType.Bitand   => Ast.Expression.UnaryOperator.AddressOf  ,
            TokenType.Not      => Ast.Expression.UnaryOperator.Not        ,
            _                  => throw new NotImplementedException()     ,
        };

        private static Ast.Expression.UnaryOperator ToPostfixUnaryOperator(TokenType tokenType) => tokenType switch
        {
            TokenType.Bitnot => Ast.Expression.UnaryOperator.Dereference,
            _ => throw new NotImplementedException(),
        };
    }
}
