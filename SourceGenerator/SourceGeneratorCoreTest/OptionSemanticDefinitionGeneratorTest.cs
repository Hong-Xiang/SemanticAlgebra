using System.Runtime.CompilerServices;
using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SemanticAlgebra;
using SemanticAlgebra.SourceGenerator;
using Xunit.Abstractions;

namespace SourceGeneratorCoreTest;

public sealed class OptionSemanticDefinitionGeneratorTest(
    ITestOutputHelper Output
)
{
    [Fact]
    public void OptionSemanticExpressionSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionSemanticExpressionSyntaxShouldWork)}.dll");
        var expr = definition.ExpressionSyntax();
        Output.WriteLine(expr.NormalizeWhitespace().ToFullString());
    }

    [Fact]
    public void OptionDataCarrierSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionDataCarrierSyntaxShouldWork)}.dll");
        var result = definition.DataCarriersSyntax();
        Output.WriteLine(result.NormalizeWhitespace().ToFullString());
    }

    [Fact]
    public void OptionDataBuilderSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionDataBuilderSyntaxShouldWork)}.dll");
        var result = definition.DataBuilderSyntax();
        Output.WriteLine(result.NormalizeWhitespace().ToFullString());
    }

    [Fact]
    public void OptionIdSemanticSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionIdSemanticSyntaxShouldWork)}.dll");
        var generator = new Semantic1IdSemanticGenerator(definition);
        var result = generator.GenerateSemanticDefinition();
        Output.WriteLine(result.NormalizeWhitespace().ToFullString());
    }

    [Fact]
    public void OptionComposeSemanticSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionIdSemanticSyntaxShouldWork)}.dll");
        var generator = new Semantic1ComposeSemanticGenerator(definition);
        var result = generator.GenerateSemanticDefinition();
        Output.WriteLine(result.NormalizeWhitespace().ToFullString());
    }

    [Fact]
    public void OptionComposeUsageMethodSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionIdSemanticSyntaxShouldWork)}.dll");
        var generator = new Semantic1ComposeSemanticGenerator(definition);
        var result = generator.GenerateSemanticUsageBrandMethod();
        Output.WriteLine(result.NormalizeWhitespace().ToFullString());
    }

    Semantic1Definition GetDefinition([CallerMemberName] string methodName = "test")
    {
        var compilation = CSharpCompilation.Create(
            methodName,
            [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
            [
                ..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );
        var tree = compilation.SyntaxTrees.Single();
        var root = tree.GetRoot();
        var target = root.DescendantNodes()
                         .OfType<InterfaceDeclarationSyntax>()
                         .Single(s => s.Identifier.Text == "ISemantic");
        var semanticModel = compilation.GetSemanticModel(tree);
        var definition = Semantic1Definition.Create(
            semanticModel,
            target,
            semanticModel.GetDeclaredSymbol(target)!
        );
        return definition;
    }

    [Fact]
    public void OptionSemanticUsageSyntaxShouldWork()
    {
        var definition = GetDefinition($"{nameof(OptionSemanticUsageSyntaxShouldWork)}.dll");
        // var compilation = CSharpCompilation.Create(
        //     $"{nameof(OptionSemanticShouldGeneratePrjMethod)}.dll",
        //     [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
        //     [
        //         ..ReferenceAssemblies.Net80,
        //         MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
        //     ]
        // );
        // var tree = compilation.SyntaxTrees.Single();
        // var root = tree.GetRoot();
        // var target = root.DescendantNodes()
        //                  .OfType<InterfaceDeclarationSyntax>()
        //                  .Single(s => s.Identifier.Text == "ISemantic");
        // var semanticModel = compilation.GetSemanticModel(tree);
        // var definition = Semantic1Definition.Create(
        //     semanticModel,
        //     target,
        //     semanticModel.GetDeclaredSymbol(target)!
        // );
        var idSemantic = new Semantic1IdSemanticGenerator(definition);
        var result = definition.BrandSemanticUsageSyntax(idSemantic);
        Output.WriteLine(result.NormalizeWhitespace().ToFullString());
    }
}