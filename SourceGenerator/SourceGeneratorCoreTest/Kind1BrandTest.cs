using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SemanticAlgebra.SourceGenerator;
using Xunit.Abstractions;

namespace SourceGeneratorCoreTest;

public sealed class Kind1BrandTest(ITestOutputHelper Output)
{
    readonly string MockAttributeDefinition = @"
namespace SemanticAlgebra.Syntax;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SemanticKind1BrandAttribute : Attribute
{
}
";

    readonly string MockISemantic1Definition = @"
namespace SemanticAlgebra;

public interface ISemantic1<out TF, in TS, out TR>
    where TF : IKind1<TF>
{
}

public interface IKind1<TF>
    where TF : IKind1<TF>
{
    static abstract ISemantic1<TF, T, IS<TF, T>> Id<T>();
}

public interface IS<in TF, out T> where TF : IKind1<TF>
{
    TR Evaluate<TR>(ISemantic1<TF, T, TR> semantic);
}
";
    readonly string CodeWithISemantic = @"
using SemanticAlgebra.Syntax;
using SemanticAlgebra;

namespace TestNamespace;

[SemanticKind1Brand]
public partial class TestOption
{
    public interface ISemantic<in TS, out TR>
        : ISemantic1<TestOption, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }
    
    public string ExistingProperty { get; set; } = ""Original"";
}
";

    [Fact]
    public void PrjMethodGenerationShouldWork()
    {
        var compilation = CSharpCompilation.Create(
            $"{nameof(PrjMethodGenerationShouldWork)}.dll",
            [
                CSharpSyntaxTree.ParseText(CodeWithISemantic, path: "code.cs"),
                CSharpSyntaxTree.ParseText(MockAttributeDefinition, path: "attr.cs"),
                CSharpSyntaxTree.ParseText(MockISemantic1Definition, path: "semantic.cs")
            ],
            references: ReferenceAssemblies.Net80);

        var generator = new SemanticKindSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator())
                                          .RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var outputs = runResult.Results;

        // Verify that sources were generated
        Assert.NotEmpty(outputs.SelectMany(r => r.GeneratedSources));

        // Should have both the partial class and the Prj extension method
        var generatedSources = outputs.SelectMany(r => r.GeneratedSources).ToList();
        Assert.True(generatedSources.Count >= 2, "Should generate at least 2 files");        // Check for Prj extension method
        var prjExtensionSource = generatedSources.FirstOrDefault(s => s.HintName.Contains("Extension.Prj"));
        Assert.True(prjExtensionSource.HintName != null, "Should generate Prj extension method");

        var prjCode = prjExtensionSource.SourceText.ToString();
        Assert.Contains("TestOptionExtension", prjCode);
        Assert.Contains("public static TestOption.ISemantic<TS, TR> Prj<TS, TR>", prjCode);
        Assert.Contains("ISemantic1<TestOption, TS, TR> semantic", prjCode);
        Assert.Contains("=> (TestOption.ISemantic<TS, TR>)semantic", prjCode);
        Assert.Contains("namespace TestNamespace;", prjCode);

        Output.WriteLine("Generated Prj Extension Method:");
        Output.WriteLine(prjCode);

        // Also output all generated sources for debugging
        Output.WriteLine($"\nTotal generated sources: {generatedSources.Count}");
        for (int i = 0; i < generatedSources.Count; i++)
        {
            var source = generatedSources[i];
            Output.WriteLine($"\n--- Generated Source {i + 1}: {source.HintName} ---");
            Output.WriteLine(source.SourceText.ToString());
        }
    }
    [Fact]
    public void RealOptionClassShouldGeneratePrjMethod()
    {
        var realOptionCode = @"
using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;
using SemanticAlgebra;

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

    public static class B
    {
        public static IS<Option, T> None<T>() => new D.None<T>();
        public static IS<Option, T> Some<T>(T value) => new D.Some<T>(value);
    }

    public static class D
    {
        public sealed record class None<T>() : IS<Option, T>
        {
            public TR Evaluate<TR>(ISemantic1<Option, T, TR> semantic)
                => semantic.Prj().None();
        }

        public sealed record class Some<T>(T Value) : IS<Option, T>
        {
            public TR Evaluate<TR>(ISemantic1<Option, T, TR> semantic)
                => semantic.Prj().Some(Value);
        }
    }
}
";

        var compilation = CSharpCompilation.Create(
            $"{nameof(RealOptionClassShouldGeneratePrjMethod)}.dll",
            [
                CSharpSyntaxTree.ParseText(realOptionCode, path: "option.cs"),
                Utility.Kind1BrandAttribute,
                CSharpSyntaxTree.ParseText(MockISemantic1Definition, path: "semantic.cs")
            ],
            references: ReferenceAssemblies.Net80);

        var generator = new SemanticKindSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator())
                                          .RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var outputs = runResult.Results;

        // Verify that sources were generated
        Assert.NotEmpty(outputs.SelectMany(r => r.GeneratedSources));

        var generatedSources = outputs.SelectMany(r => r.GeneratedSources).ToList();
        Assert.True(generatedSources.Count >= 2, "Should generate at least 2 files");

        // Check for Prj extension method
        var prjExtensionSource = generatedSources.FirstOrDefault(s => s.HintName.Contains("Extension.Prj"));
        Assert.True(prjExtensionSource.HintName != null, "Should generate Prj extension method");

        var prjCode = prjExtensionSource.SourceText.ToString();
        Assert.Contains("OptionExtension", prjCode);
        Assert.Contains("public static Option.ISemantic<TS, TR> Prj<TS, TR>", prjCode);
        Assert.Contains("ISemantic1<Option, TS, TR> semantic", prjCode);
        Assert.Contains("=> (Option.ISemantic<TS, TR>)semantic", prjCode);
        Assert.Contains("namespace SemanticAlgebra.Option;", prjCode);

        Output.WriteLine("Generated Prj Extension Method for Real Option:");
        Output.WriteLine(prjCode);
    }
    [Fact]
    public void ClassWithoutISemanticShouldNotGeneratePrjMethod()
    {
        var codeWithoutISemantic = @"
using SemanticAlgebra.Syntax;

namespace TestNamespace;

[SemanticKind1Brand]
public partial class ClassWithoutISemantic
{
    public string ExistingProperty { get; set; } = ""Original"";
}
";

        var compilation = CSharpCompilation.Create(
            $"{nameof(ClassWithoutISemanticShouldNotGeneratePrjMethod)}.dll",
            [
                CSharpSyntaxTree.ParseText(codeWithoutISemantic, path: "code.cs"),
                CSharpSyntaxTree.ParseText(MockAttributeDefinition, path: "attr.cs"),
                CSharpSyntaxTree.ParseText(MockISemantic1Definition, path: "semantic.cs")
            ],
            references: ReferenceAssemblies.Net80);

        var generator = new SemanticKindSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator())
                                          .RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var outputs = runResult.Results;

        // Verify that sources were generated
        Assert.NotEmpty(outputs.SelectMany(r => r.GeneratedSources));

        var generatedSources = outputs.SelectMany(r => r.GeneratedSources).ToList();

        // Should only generate the partial class, not the Prj extension
        Assert.Single(generatedSources);

        // Check that NO Prj extension method was generated
        var prjExtensionSource = generatedSources.FirstOrDefault(s => s.HintName.Contains("Extension.Prj"));
        Assert.True(prjExtensionSource.HintName == null, "Should NOT generate Prj extension method when ISemantic interface is missing");

        // Should still generate the partial class
        var partialClassSource = generatedSources.First();
        var partialCode = partialClassSource.SourceText.ToString();
        Assert.Contains("partial class ClassWithoutISemantic", partialCode);
        Assert.Contains("SourceGenTest", partialCode);

        Output.WriteLine("Generated Partial Class (without Prj method):");
        Output.WriteLine(partialCode);
    }
}
