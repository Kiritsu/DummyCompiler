using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DummyCompiler.Records;

namespace DummyCompiler
{
    public class CompilerBuilder
    {
        /// <summary>
        /// Grammar to be used for the built instance of the compiler.
        /// </summary>
        public ImmutableArray<GrammarRule> Grammar { get; private set; }
        
        /// <summary>
        /// Language keywords to be used for the built instance of the compiler.
        /// </summary>
        public ImmutableArray<string> LanguageKeywords { get; private set; }
        
        /// <summary>
        /// Dictionary to be used for the built instance of the compiler.
        /// </summary>
        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> Dictionary { get; private set; }
        
        /// <summary>
        /// Code to be used for the built instance of the compiler.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Defines the grammar to be used for the built instance of the compiler.
        /// </summary>
        /// <param name="filePath">Path to the file representing the grammar.</param>
        public CompilerBuilder WithGrammar(string filePath)
        {
            var content = File.ReadAllLines(filePath);
            
            var grammar = content
                .Select(line => line.Split('\t'))
                .Select(elements =>
                    new GrammarRule(
                        int.Parse(elements[0]),
                        elements[1],
                        elements[2].Split(' ').ToImmutableArray()))
                .ToList();

            Grammar = grammar.ToImmutableArray();

            return this;
        }

        /// <summary>
        /// Defines the languages keywords to be used for the built instance of the compiler.
        /// </summary>
        /// <param name="filePath">Path to the file representing the language keywords.</param>
        public CompilerBuilder WithLanguageKeywords(string filePath)
        {
            LanguageKeywords = File.ReadAllText(filePath)
                .Split(' ')
                .ToImmutableArray();
            
            return this;
        }
        
        /// <summary>
        /// Defines the dictionary to be used for the built instance of the compiler.
        /// </summary>
        /// <param name="filePath">Path to the file representing the dictionary.</param>
        public CompilerBuilder WithDictionary(string filePath)
        {
            var rawDictionary = File.ReadAllLines(filePath);
            var scopedDictionary = rawDictionary.Select(x => x.Split('\t')).ToArray();

            var dictionary = new Dictionary<string, ReadOnlyDictionary<string, string>>();
            for (var i = 0; i < scopedDictionary[0].Length; i++)
            {
                var currentToken = scopedDictionary[0][i];
                var currentDictionary = new Dictionary<string, string>();
                for (var j = 1; j < scopedDictionary.Length; j++)
                {
                    currentDictionary.Add(scopedDictionary[j][0], scopedDictionary[j][i]);
                }

                dictionary.Add(currentToken, new ReadOnlyDictionary<string, string>(currentDictionary));
            }

            Dictionary = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>>(dictionary);

            return this;
        }

        /// <summary>
        /// Defines the code to be used for the built instance of the compiler.
        /// </summary>
        /// <param name="filePath">Path to the file representing the code.</param>
        public CompilerBuilder WithCode(string filePath)
        {
            Code = File.ReadAllText(filePath);

            return this;
        }

        /// <summary>
        /// Creates a new instance of a compiler.
        /// </summary>
        public Compiler Build()
        {
            return new(Grammar, LanguageKeywords, Dictionary, Code);
        }
    }
}