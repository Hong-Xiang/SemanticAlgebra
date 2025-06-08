using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SemanticAlgebra;
using SemanticAlgebra.SourceGenerator;
using Xunit.Abstractions;

namespace SourceGeneratorCoreTest;

public sealed class Semantic1GeneratorTest(ITestOutputHelper Output)
{
    [Fact]
    public void OptionSemanticShouldGeneratePrjMethod()
    {
        var compilation = CSharpCompilation.Create(
            $"{nameof(OptionSemanticShouldGeneratePrjMethod)}.dll",
            [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
            [
                ..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );


        var generator = new Semantic1Generator();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator())
                                          .RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var outputs = runResult.Results;

        var generatedSources = outputs.SelectMany(r => r.GeneratedSources).ToList();
        Assert.NotEmpty(generatedSources);
        foreach (var generatedSource in generatedSources)
        {
            Output.WriteLine($"{generatedSource.HintName}: =====");
            Output.WriteLine(generatedSource.SourceText.ToString());
        }
    }

    [Fact]
    public Task StateSemanticShouldGeneratePrjMethod()
    {
        var compilation = CSharpCompilation.Create(
            $"{nameof(StateSemanticShouldGeneratePrjMethod)}.dll",
            [CSharpSyntaxTree.ParseText(Utility.State2BrandCode, path: "State.cs")],
            [
                ..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );


        var generator = new Semantic1Generator();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator())
                                          .RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var outputs = runResult.Results;

        var generatedSources = outputs.SelectMany(r => r.GeneratedSources).ToList();
        Assert.NotEmpty(generatedSources);
        var source = generatedSources.Single(s => s.HintName.Contains("Prj"));
        var code = source.SourceText.ToString();
        Output.WriteLine(code);
        return Verify(code);
    }
}