using System;
using System.Collections.Immutable;

namespace DummyCompiler.Exceptions
{
    public class UnreachableCodeException : Exception
    {
        /// <summary>
        /// Tokens that can't be parsed because it is unreachable.
        /// </summary>
        public ImmutableArray<string> RemainingTokens { get; }

        public UnreachableCodeException(ImmutableArray<string> remainingTokens) 
            : base("End of code has been passed. Next tokens are unreachable.")
        {
            RemainingTokens = remainingTokens;
        }
    }
}