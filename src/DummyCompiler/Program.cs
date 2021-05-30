using System;
using DummyCompiler.Exceptions;
using DummyCompiler.Extensions;

namespace DummyCompiler
{
    public class Program
    {
        public static void Main()
        {
            var compiler = new CompilerBuilder()
                .WithCode(@"code.txt")
                .WithLanguageKeywords(@"keywords.txt")
                .WithGrammar(@"rules.txt")
                .WithDictionary(@"dictionary.csv")
                .Build();
            
            compiler.ParseTokens();

            try
            {
                while (!compiler.IsDone())
                {
                    compiler.CompileNextToken();
                    
                    PrintCompilerState(compiler);
                    WaitForNextStep();
                }
                
                PrintCompilerState(compiler);
                Console.WriteLine("Compilation over.");
            }
            catch (UnreachableCodeException ex)
            {
                Console.WriteLine($"{ex.Message} >> {string.Join(", ", ex.RemainingTokens)}");
            }
            catch (UnknownRuleException ex)
            {
                Console.WriteLine($"{ex.Message} >> pop: {ex.Pop} | token: {ex.Token} | rule: {ex.Rule}");
            }
            catch (UnexpectedTokenException ex)
            {
                Console.WriteLine($"{ex.Message} >> token: {ex.Token}");
            }
        }

        public static void PrintCompilerState(Compiler compiler)
        {
            Console.Clear();
            Console.WriteLine($"OUPUT: {compiler.GetOutput()}");
            Console.WriteLine($"INPUT: {compiler.GetRemainingInput()}");
            Console.WriteLine(compiler.GetStackViewAscii());
        }

        public static void WaitForNextStep()
        {
            Console.WriteLine("Press a key to continue to the next token...");
            Console.ReadKey();
        }
    }
}