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
        /// Happens, when a syntax error occurs.
        /// </summary>
        public event EventHandler<Syntax.Error.ISyntaxError> OnError;

        /// <summary>
        /// Lexes the tokens for a given file path.
        /// </summary>
        /// <param name="path">The path for the file to lex.</param>
        /// <returns>The resulting list of tokens.</returns>
        [QueryChannel(nameof(OnError))]
        public IValueList<Syntax.Token> LexTokens(string path);

        /// <summary>
        /// Parses the given file into a parse tree.
        /// </summary>
        /// <param name="path">The path for the file to parse.</param>
        /// <returns>The parsed file declaration.</returns>
        [QueryChannel(nameof(OnError))]
        public Syntax.ParseTree.Declaration.File ParseFile(string path);

        // TODO: Shouldn't this take the parse-tree instead?
        // Both makes sense
        /// <summary>
        /// Parses the given file into an AST.
        /// </summary>
        /// <param name="path">The path for the file to parse.</param>
        /// <returns>The parsed AST.</returns>
        [QueryChannel(nameof(OnError))]
        public Syntax.Ast.Declaration.File ParseFileToAst(string path);

        // TODO: Shouldn't this take the AST instead?
        // Both makes sense
        /// <summary>
        /// Parses the given file into an AST that has all sugar removed.
        /// </summary>
        /// <param name="path">The path for the file to parse.</param>
        /// <returns>The parsed and desugared AST.</returns>
        [QueryChannel(nameof(OnError))]
        public Syntax.Ast.Declaration.File ParseFileToDesugaredAst(string path);
    }
}
