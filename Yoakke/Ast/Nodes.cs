using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Syntax;

namespace Yoakke.Ast
{
    /// <summary>
    /// Base class for all AST nodes.
    /// </summary>
    abstract class Node
    {
    }

    /// <summary>
    /// Base class for all statements.
    /// </summary>
    abstract class Statement : Node
    {
    }

    /// <summary>
    /// Base class for all statements, that can have order-independence.
    /// They are called declarations.
    /// </summary>
    abstract class Declaration : Statement
    {
    }

    /// <summary>
    /// Base class for all expressions, that result in a value and can participate in other expressions.
    /// </summary>
    abstract class Expression : Node
    {
    }
}
