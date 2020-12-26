namespace Yoakke.Syntax.Ast
{
    // TODO: Doc
    public class Cloner : Transformator
    {
        public Declaration Clone(Declaration decl) => Transform(decl);
        public Statement Clone(Statement stmt) => Transform(stmt);
        public Expression Clone(Expression expr) => Transform(expr);

        protected override Node? Visit(Expression.Identifier ident) => 
            new Expression.Identifier(ident.ParseTreeNode, ident.Name);
        protected override Node? Visit(Expression.Literal lit) => 
            new Expression.Literal(lit.ParseTreeNode, lit.Type, lit.Value);
    }
}
