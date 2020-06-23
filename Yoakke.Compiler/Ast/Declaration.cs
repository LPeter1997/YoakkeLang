using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Syntax;

namespace Yoakke.Compiler.Ast
{
    partial class Declaration
    {
        /// <summary>
        /// A list of top-level declarations.
        /// </summary>
        public class Program : Declaration
        {
            /// <summary>
            /// The list of <see cref="Declaration"/>s the <see cref="Program"/> consists of.
            /// </summary>
            public List<Declaration> Declarations { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Program"/>.
            /// </summary>
            /// <param name="declarations">The list of <see cref="Declaration"/>s the program consists of.</param>
            public Program(List<Declaration> declarations)
            {
                Declarations = declarations;
            }

            public override Declaration CloneDeclaration() =>
                new Program(Declarations.Select(x => x.CloneDeclaration()).ToList());
        }

        /// <summary>
        /// Anything in the form of
        /// ```
        /// const Name = Value;
        /// ```
        /// or
        /// ```
        /// const Name: Type = Value;
        /// ```
        /// </summary>
        public class ConstDef : Declaration
        {
            /// <summary>
            /// The name of the constant defined.
            /// </summary>
            public Token Name { get; set; }
            /// <summary>
            /// The type of the constant defined. Can be null, which means that it will be inferred.
            /// </summary>
            public Expression? Type { get; set; }
            /// <summary>
            /// The value the constant gets defined with.
            /// </summary>
            public Expression Value { get; set; }

            /// <summary>
            /// The <see cref="Symbol"/> this constant defines.
            /// </summary>
            public Symbol.Const? Symbol { get; set; }

            /// <summary>
            /// Initializes a new <see cref="ConstDef"/>.
            /// </summary>
            /// <param name="name">The name of the constant defined.</param>
            /// <param name="type">The type of the constant defined.</param>
            /// <param name="value">The value the constant gets defined with.</param>
            public ConstDef(Token name, Expression? type, Expression value)
            {
                Name = name;
                Type = type;
                Value = value;
            }

            public override Declaration CloneDeclaration() =>
                new ConstDef(Name, Type?.CloneExpression(), Value.CloneExpression());
        }
    }
}
