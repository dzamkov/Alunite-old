using System;
using System.Collections.Generic;
using System.Linq;

namespace Alunite
{
    /// <summary>
    /// A matrix that relates two objects of a common base to a function that generates a result of the desired type.
    /// </summary>
    public class MatchMatrix<TBase, TResult>
        where TBase : class
    {
        public MatchMatrix(Match<TBase, TBase> Default)
        {
            this._Rules = new Stack<MatchMatrix<TBase, TResult>._Rule>();
            this._Rules.Push(new _Rule.Special<TBase, TBase>(Default));
        }

        /// <summary>
        /// Adds a symmetric rule for the matrix for the two specialized types. The most recently added rules have the highest priority.
        /// </summary>
        public void AddRule<TA, TB>(Match<TA, TB> Rule)
            where TA : class, TBase
            where TB : class, TBase
        {
            this._Rules.Push(new _Rule.Special<TA, TB>(Rule));
        }

        /// <summary>
        /// Gets the result defined by the matrix for the two specified objects.
        /// </summary>
        public TResult GetResult(TBase A, TBase B)
        {
            TResult res = default(TResult);
            foreach (_Rule r in this._Rules)
            {
                if (r.Try(A, B, ref res))
                {
                    return res;
                }
            }
            return res;
        }

        /// <summary>
        /// A function that produces a result given two objects of a specialized type from the common base.
        /// </summary>
        public delegate TResult Match<TA, TB>(TA A, TB B)
            where TA : TBase
            where TB : TBase;

        /// <summary>
        /// Represents a matching rule.
        /// </summary>
        private abstract class _Rule
        {
            /// <summary>
            /// Tries to apply this rule to the specified object pair.
            /// </summary>
            public abstract bool Try(TBase A, TBase B, ref TResult Result);

            public class Special<TA, TB> : _Rule
                where TA : class, TBase
                where TB : class, TBase
            {
                public Special(Match<TA, TB> Match)
                {
                    this.Match = Match;
                }

                public override bool Try(TBase A, TBase B, ref TResult Result)
                {
                    TA a = A as TA;
                    TB b = B as TB;
                    if (a != null && b != null)
                    {
                        Result = this.Match(a, b);
                        return true;
                    }
                    a = B as TA;
                    b = A as TB;
                    if (a != null && b != null)
                    {
                        Result = this.Match(a, b);
                        return true;
                    }
                    return false;
                }

                /// <summary>
                /// The match for this rule.
                /// </summary>
                public Match<TA, TB> Match;
            }
        }

        private Stack<_Rule> _Rules;
    }
}