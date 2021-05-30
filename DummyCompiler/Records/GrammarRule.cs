using System.Collections.Immutable;

namespace DummyCompiler.Records
{
    /// <summary>
    /// Represents a new Grammar Rule.
    /// </summary>
    /// <param name="Number">Number of the rule.</param>
    /// <param name="Name">Name of the rule.</param>
    /// <param name="Grammar">Elements defined under that rule.</param>
    public record GrammarRule(int Number, string Name, ImmutableArray<string> Grammar);
}