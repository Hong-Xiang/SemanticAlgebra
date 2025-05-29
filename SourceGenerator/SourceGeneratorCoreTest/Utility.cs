using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace SourceGeneratorCoreTest;

internal static class Utility
{
    public static SyntaxTree Kind1BrandAttribute = CSharpSyntaxTree.ParseText(
@"
namespace SemanticAlgebra.Syntax;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SemanticKind1BrandAttribute : Attribute
{
}
", path: "syntax.cs");

    public static IEnumerable<GeneratedSourceResult> GeneratedSources(this GeneratorDriverRunResult? result)
        => result is null ? [] : result.Results.SelectMany(r => r.GeneratedSources);

    public static readonly string OptionBrandCode = """
using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Option;

[SemanticKind1Brand]
public sealed partial class Option
    : IMonad<Option>
{
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Option, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }
}
""";
}
