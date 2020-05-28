using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Operations for type-inference and unification.
    /// </summary>
    static class Unifier
    {
        /// <summary>
        /// Tries to unify the given types.
        /// </summary>
        /// <param name="t1">The first <see cref="Type"/> to unify.</param>
        /// <param name="t2">The second <see cref="Type"/> to unify.</param>
        public static void Unify(Type t1, Type t2)
        {
            void UnifyVarVar(TypeVariable v1, TypeVariable v2)
            {
                if (ReferenceEquals(v1, v2)) return;
                v2.SubstituteFor(v1);
            }

            void UnifyCtorVar(TypeConstructor c1, TypeVariable v2)
            {
                if (c1.Contains(v2))
                {
                    throw new NotImplementedException("Type-recursion!");
                }
                v2.SubstituteFor(c1);
            }

            void UnifyCtorCtor(TypeConstructor c1, TypeConstructor c2)
            {
                if (c1.Name != c2.Name)
                {
                    throw new NotImplementedException("Type mismatch!");
                }
                if (c1.Subtypes.Count != c2.Subtypes.Count)
                {
                    throw new NotImplementedException("Subtype amount mismatch!");
                }
                for (int i = 0; i < c1.Subtypes.Count; ++i)
                {
                    Unify(c1.Subtypes[i], c2.Subtypes[i]);
                }
            }

            t1 = t1.Substitution;
            t2 = t2.Substitution;
            if (t1 is TypeVariable v1)
            {
                if (t2 is TypeVariable v2)
                {
                    UnifyVarVar(v1, v2);
                    return;
                }
                if (t2 is TypeConstructor c2)
                {
                    UnifyCtorVar(c2, v1);
                    return;
                }
            }
            if (t1 is TypeConstructor c1)
            {
                if (t2 is TypeVariable v2)
                {
                    UnifyCtorVar(c1, v2);
                    return;
                }
                if (t2 is TypeConstructor c2)
                {
                    UnifyCtorCtor(c1, c2);
                    return;
                }
            }
            throw new NotImplementedException();
        }
    }
}
