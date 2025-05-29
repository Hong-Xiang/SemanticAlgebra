using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SemanticAlgebra.SourceGenerator;

namespace SourceGeneratorCoreTest;

public class Kind1BrandTest
{    readonly string MockAttributeDefinition = @"
namespace SemanticAlgebra.Syntax;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SemanticKind1BrandAttribute : Attribute
{
}
";readonly string Code = @"
using SemanticAlgebra.Syntax;

namespace LambdaLang;

[SemanticKind1Brand]
public partial class TestClass
{
    public string ExistingProperty { get; set; } = ""Original"";
    
    public void TestMethod()
    {
        Console.WriteLine($""Existing: {ExistingProperty}"");
        Console.WriteLine($""Generated: {SourceGenTest}"");
        Console.WriteLine($""Generated: {SourceGenTest2}"");
    }
}
";    [Fact]
    public void BasicKind1BrandDetectionShouldWork()
    {
        var compilation = CSharpCompilation.Create(
            $"{nameof(BasicKind1BrandDetectionShouldWork)}.dll",
            [CSharpSyntaxTree.ParseText(Code, path: "code.cs"), CSharpSyntaxTree.ParseText(MockAttributeDefinition, path: "attr.cs")],
            references: ReferenceAssemblies.Net80);
        var tree = compilation.SyntaxTrees[0];
        var root = tree.GetCompilationUnitRoot();
        var model = compilation.GetSemanticModel(tree);


        var generator = new SemanticKindSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator())
                                          .RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var outputs = runResult.Results;
        
        // Verify that sources were generated
        Assert.NotEmpty(outputs.SelectMany(r => r.GeneratedSources));
        
        // Verify the content of generated source
        var generatedSource = outputs.SelectMany(r => r.GeneratedSources).First();
        var generatedCode = generatedSource.SourceText.ToString();
        
        // Check that the generated code contains expected elements
        Assert.Contains("partial class TestClass", generatedCode);
        Assert.Contains("public string SourceGenTest", generatedCode);
        Assert.Contains("public string SourceGenTest2", generatedCode);
        Assert.Contains("namespace LambdaLang;", generatedCode);
    }
}
