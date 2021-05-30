using System;

namespace DummyCompiler.Exceptions
{
    public class UnexpectedTokenException : Exception
    {
        /// <summary>
        /// Gets the token that was unexpected.
        /// </summary>
        public string Token { get; }

        public UnexpectedTokenException(string token, string message) : base(message)
        {
            Token = token;
        }
        
        public UnexpectedTokenException(string token) : base("The current token isn't valid in the current context.")
        {
            Token = token;
        }
    }
}