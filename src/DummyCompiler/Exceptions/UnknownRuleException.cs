using System;

namespace DummyCompiler.Exceptions
{
    public class UnknownRuleException : Exception
    {
        /// <summary>
        /// Gets the rule that was tried to be reached.
        /// </summary>
        public int Rule { get; }
        
        /// <summary>
        /// Gets the token that lead to finding an invalid rule.
        /// </summary>
        public string Token { get; }
        
        /// <summary>
        /// Gets the poped element that lead to finding an invalid rule.
        /// </summary>
        public string Pop { get; }

        public UnknownRuleException(int rule, string pop, string token) 
            : base("No rule could be found within the current context.")
        {
            Rule = rule;
            Pop = pop;
            Token = token;
        }
    }
}