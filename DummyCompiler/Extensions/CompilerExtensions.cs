using System.Linq;
using System.Text;

namespace DummyCompiler.Extensions
{
    public static class CompilerExtensions
    {
        /// <summary>
        /// Gets the stack of the compiler as a fancy ASCII view.
        /// </summary>
        /// <param name="compiler">Stack's compiler.</param>
        public static string GetStackViewAscii(this Compiler compiler)
        {
            var list = compiler.Stack.ToList();
            var builder = new StringBuilder();

            if (list.Count <= 0)
            {
                return builder.ToString();
            }

            builder.AppendLine("=======");
            for (var i = 0; i < 7 - list.Count; i++)
            {
                builder.AppendLine("|     |");
                builder.AppendLine("|     |");
                builder.AppendLine("|     |");
                builder.AppendLine("=======");
            }
            
            foreach (var element in list)
            {
                builder.AppendLine("|     |");
                builder.Append('|').Append(element.PadLeft(5)).AppendLine("|");
                builder.AppendLine("|     |");
                builder.AppendLine("=======");
            }

            return builder.ToString();
        }
    }
}