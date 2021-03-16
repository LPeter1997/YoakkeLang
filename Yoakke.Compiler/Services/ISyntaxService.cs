using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Dependency;

namespace Yoakke.Compiler.Services
{
    /// <summary>
    /// Service definition for syntax analysis.
    /// </summary>
    [QueryGroup]
    public partial interface ISyntaxService
    {
        /// <summary>
        /// Lexes the tokens for a given file path.
        /// </summary>
        /// <param name="path">The path for the file to lex.</param>
        /// <returns>The resulting list of tokens.</returns>
        public IValueList<Syntax.Token> LexTokens(string path);

        /// <summary>
        /// Parses the given file into a parse tree.
        /// </summary>
        /// <param name="path">The path for the file to parse.</param>
        /// <returns>The parsed file declaration.</returns>
        public Syntax.ParseTree.Declaration.File ParseFile(string path);

        /// <summary>
        /// Parses the given file into an AST.
        /// </summary>
        /// <param name="path">The path for the file to parse.</param>
        /// <returns>The parsed AST.</returns>
        public Syntax.Ast.Declaration.File ParseFileToAst(string path);
    }
}
