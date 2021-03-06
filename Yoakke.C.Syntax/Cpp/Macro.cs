﻿using System.Collections.Generic;

namespace Yoakke.C.Syntax.Cpp
{
    /// <summary>
    /// Represents a macro invocation.
    /// </summary>
    public class MacroCall
    {
        /// <summary>
        /// The called <see cref="Macro"/>.
        /// </summary>
        public readonly Macro Macro;
        /// <summary>
        /// The call-site identifier <see cref="Token"/> that identifies the <see cref="Macro"/>.
        /// </summary>
        public readonly Token CallSiteIdentifier;
        /// <summary>
        /// The arguments passed to the call.
        /// </summary>
        public readonly IDictionary<string, IList<Token>> Arguments;

        // TODO: Doc
        public MacroCall(
            Macro macro,
            Token callSiteIdentifier, 
            IDictionary<string, IList<Token>> arguments)
        {
            Macro = macro;
            CallSiteIdentifier = callSiteIdentifier;
            Arguments = arguments;
        }
    }

    /// <summary>
    /// The base class for every macro.
    /// </summary>
    public abstract class Macro
    {
        /// <summary>
        /// True, if '()' is needed for expansion.
        /// </summary>
        public abstract bool NeedsParens { get; }
        /// <summary>
        /// True, if this is a variadic-argument macro.
        /// </summary>
        public abstract bool IsVariadic { get; }
        /// <summary>
        /// The fixed, named list of parameters.
        /// </summary>
        public abstract IReadOnlyList<string> Parameters { get; }
    }

    /// <summary>
    /// A user-defined macro.
    /// </summary>
    public class UserMacro : Macro
    {
        public override bool NeedsParens { get; }
        public override bool IsVariadic { get; }
        public override IReadOnlyList<string> Parameters { get; }
        // TODO: Doc
        public IReadOnlyList<Token> Substitution;

        // TODO: Doc
        public UserMacro(
            bool needsParens, 
            bool isVariadic, 
            IReadOnlyList<string> parameters, 
            IReadOnlyList<Token> substitution)
        {
            NeedsParens = needsParens;
            IsVariadic = isVariadic;
            Parameters = parameters;
            Substitution = substitution;
        }
    }
}
