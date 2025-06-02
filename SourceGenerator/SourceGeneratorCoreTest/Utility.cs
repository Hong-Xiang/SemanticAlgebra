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
", path: "SemanticKind1BrandAttribute.cs");
    
    public static IEnumerable<GeneratedSourceResult> GeneratedSources(this GeneratorDriverRunResult? result)
        => result is null ? [] : result.Results.SelectMany(r => r.GeneratedSources);

    public static readonly string State2BrandCode = 
        
"""
using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.State;

public sealed partial class State
    : IMonad<State>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<State, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }
}
""";

    public static readonly string OptionBrandCode = """
using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Option;

public sealed partial class Option
    : IMonad<Option>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Option, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }
    
    public void Foo() { }
}
""";
}
