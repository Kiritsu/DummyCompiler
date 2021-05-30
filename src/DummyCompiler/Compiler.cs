using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using DummyCompiler.Exceptions;
using DummyCompiler.Extensions;
using DummyCompiler.Records;

namespace DummyCompiler
{
    public class Compiler
    {
        /// <summary>
        /// Represents the stack of our compiler.
        /// </summary>
        public Stack<string> Stack { get; }
        
        /// <summary>
        /// Represents the different grammar rules.
        /// </summary>
        public ImmutableArray<GrammarRule> Grammar { get; }
        
        /// <summary>
        /// Gets the language keywords (that will be forbidden in other cases).
        /// </summary>
        public ImmutableArray<string> LanguageKeywords { get; }
        
        /// <summary>
        /// Gets the dictionary that the compiler will use to determine what rule to use or whether to pop, etc.
        /// </summary>
        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> Dictionary { get; }

        /// <summary>
        /// Gets the raw uncleaned code.
        /// </summary>
        public string RawCode { get; }
        
        /// <summary>
        /// Gets the code without unwanted chars and trimmed.
        /// </summary>
        public string Code => _code ??= RawCode
            .Replace("\r", "")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .Trim();
        private string _code;

        /// <summary>
        /// Gets the code as tokens.
        /// </summary>
        public IEnumerable<string> RawTokens => _rawTokens ??= Code.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
        private IEnumerable<string> _rawTokens;
        
        /// <summary>
        /// Gets the tokens parsed as grammar. Variables and constants are changed into nb and id.
        /// </summary>
        public ImmutableArray<string> GrammarTokens { get; private set; }

        /// <summary>
        /// Represents the index of the current state of the compilation.
        /// </summary>
        private volatile int _index;
        
        /// <summary>
        /// Represents the current output of the compilation.
        /// </summary>
        private readonly StringBuilder _output;

        /// <summary>
        /// Indicates whether the compilation is over or not.
        /// </summary>
        private bool _isCompileOver;
        
        /// <summary>
        /// Creates a new instance of a compiler with its given rules and dictionary.
        /// </summary>
        /// <param name="grammar">Grammar to use.</param>
        /// <param name="languageKeywords">Language keywords.</param>
        /// <param name="dictionary">Compiler dictionary.</param>
        /// <param name="rawCode">Raw code to compile.</param>
        public Compiler(
            ImmutableArray<GrammarRule> grammar, 
            ImmutableArray<string> languageKeywords, 
            ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> dictionary, 
            string rawCode)
        {
            Stack = new Stack<string>();
            Grammar = grammar;
            LanguageKeywords = languageKeywords;
            Dictionary = dictionary;
            RawCode = rawCode;

            _index = 0;
            _output = new StringBuilder();
        }

        /// <summary>
        /// Parse the code and creates tokens from it that can be used to check against the compiler dictionary.
        /// </summary>
        /// <exception cref="UnexpectedTokenException">Thrown when an expected value is found.</exception>
        public void ParseTokens()
        {
            var grammarTokens = new List<string>();
            
            foreach (var token in RawTokens)
            {
                if (token.IsNumber(out _))
                {
                    grammarTokens.Add("nb");
                    continue;
                }

                var isLanguageReserved = LanguageKeywords.Contains(token);
                switch (isLanguageReserved)
                {
                    case false when token.All(char.IsLetter):
                        grammarTokens.Add("id");
                        break;
                    case true:
                        grammarTokens.Add(token);
                        break;
                    default:
                        throw new UnexpectedTokenException(token);
                }
            }

            GrammarTokens = grammarTokens.ToImmutableArray();
            
            Stack.Push("$");
            Stack.Push("P");
        }

        /// <summary>
        /// Compiles the next token.
        /// </summary>
        /// <exception cref="UnreachableCodeException">Thrown when a token indicating the end of the code was parsed previously.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the tokens haven't been properly parsed.</exception>
        /// <exception cref="UnexpectedTokenException">Thrown when the current token was unexpected or unknown.</exception>
        /// <exception cref="UnknownRuleException">Thrown when the rule couldn't be found within the current context.</exception>
        public void CompileNextToken()
        {
            if (_isCompileOver && _index < GrammarTokens.Length)
            {
                throw new UnreachableCodeException(GrammarTokens.Skip(_index).ToImmutableArray());
            }
            
            if (GrammarTokens.Length == 0)
            {
                throw new InvalidOperationException("Grammar tokens must be parsed first.");
            }

            if (_index == GrammarTokens.Length)
            {
                throw new InvalidOperationException("Compilation is over already.");
            }

            var token = GrammarTokens[_index];
            var pop = Stack.Pop();

            if (!Dictionary.TryGetValue(token, out var popDictionary))
            {
                throw new UnexpectedTokenException(token, 
                    "The token was not registered in the dictionary.");
            }

            if (!popDictionary.TryGetValue(pop, out var element))
            {
                throw new UnexpectedTokenException(pop, 
                    "Couldn't manage to associate the pop within the current token context.");
            }

            _output.Append(element).Append(' ');
            
            if (int.TryParse(element, out var ruleNumber))
            {
                // we try to find a rule according to the dictionary
                var rule = Grammar.FirstOrDefault(x => x.Number == ruleNumber);
                if (rule is null)
                {
                    throw new UnknownRuleException(ruleNumber, pop, token);
                }

                // we push every grammar element to the stack but epsilon
                for (var i = rule.Grammar.Length; i > 0; i--)
                {
                    var subToken = rule.Grammar[i - 1];
                    if (subToken != "ε")
                    {
                        Stack.Push(subToken);
                    }
                }
            }
            else switch (element)
            {
                // we need to increase the index to parse the next element at next call
                case "pop":
                    Interlocked.Increment(ref _index);
                    break;
                // the program is considered over. everything after is unreachable and so invalid
                case "ACC":
                    Interlocked.Increment(ref _index);
                    _isCompileOver = true;
                    break;
            }

            // check if it's over
            if (IsDone())
            {
                _isCompileOver = true;
            }
        }

        /// <summary>
        /// Gets the current compiler output.
        /// </summary>
        public string GetOutput()
        {
            return _output.ToString();
        }

        /// <summary>
        /// Gets the remaining input.
        /// </summary>
        public string GetRemainingInput()
        {
            return string.Join(" ", GrammarTokens.Skip(_index));
        }

        /// <summary>
        /// Gets whether the compiler is done compiling or not.
        /// </summary>
        public bool IsDone()
        {
            return _index == GrammarTokens.Length;
        }
    }
}