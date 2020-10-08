using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.C.Syntax.Cpp
{
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

        /// <summary>
        /// Expands this macro.
        /// </summary>
        /// <param name="callSiteIdent">The call-site identifier <see cref="Token"/> that referenced this macro.</param>
        /// <param name="namedArgs">The argument dictionary mapping argument names to <see cref="Token"/> sequences.</param>
        /// <param name="variadicArgs">The list of variadic <see cref="Token"/> argument sequences.</param>
        /// <returns>The expanded <see cref="IEnumerable{Token}"/>.</returns>
        public abstract IEnumerable<Token> Expand(
            Token callSiteIdent, 
            IDictionary<string, IList<Token>> namedArgs,
            IList<IList<Token>> variadicArgs);
    }

    /// <summary>
    /// A simple macro that requires no arguments and can be driven with a <see cref="Func{Token, IEnumerable{Token}}"/>.
    /// </summary>
    public class FuncLabelMacro : Macro
    {
        public override bool NeedsParens => false;
        public override bool IsVariadic => false;
        public override IReadOnlyList<string> Parameters { get; } = new string[] { };

        private readonly Func<Token, IEnumerable<Token>> expander;

        /// <summary>
        /// Initializes a new <see cref="FuncLabelMacro"/>.
        /// </summary>
        /// <param name="expander">The <see cref="Expander"/> function.</param>
        public FuncLabelMacro(Func<Token, IEnumerable<Token>> expander)
        {
            this.expander = expander;
        }

        public override IEnumerable<Token> Expand(
            Token callSiteIdent, 
            IDictionary<string, IList<Token>> namedArgs, 
            IList<IList<Token>> variadicArgs)
        {
            Debug.Assert(namedArgs.Count == 0);
            Debug.Assert(variadicArgs.Count == 0);
            return expander(callSiteIdent);
        }
    }

    /// <summary>
    /// A user-defined macro.
    /// </summary>
    public class UserMacro : Macro
    {
        public override bool NeedsParens { get; }
        public override bool IsVariadic { get; }
        public override IReadOnlyList<string> Parameters { get; }

        private IReadOnlyList<Token> substitution;

        public UserMacro(bool needsParens, bool isVariadic, IReadOnlyList<string> parameters, IReadOnlyList<Token> substitution)
        {
            NeedsParens = needsParens;
            IsVariadic = isVariadic;
            Parameters = parameters;
            this.substitution = substitution;
        }

        public override IEnumerable<Token> Expand(
            Token callSiteIdent, 
            IDictionary<string, IList<Token>> namedArgs, 
            IList<IList<Token>> variadicArgs)
        {
            Debug.Assert(Parameters.Count == namedArgs.Count);
            Debug.Assert(IsVariadic || variadicArgs.Count == 0);
            
            for (int i = 0; i < substitution.Count;)
            {
                // TODO: Handle #, ##, __VA_ARGS__

                var t = substitution[i];
                if (namedArgs.TryGetValue(t.Value, out var argValue))
                {
                    foreach (var sub in argValue) yield return sub;
                }
                else
                {
                    // Just a simple argument
                    yield return t;
                }
                ++i;
            }
        }
    }
}
