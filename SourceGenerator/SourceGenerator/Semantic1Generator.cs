using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SemanticAlgebra.SourceGenerator;

[Generator]
public class Semantic1Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var semantic1 = context.SyntaxProvider.ForAttributeWithMetadataName(
            SemanticAlgebraAbstraction.Semantic1AttributeName,
            static (s, _) => s is InterfaceDeclarationSyntax,
            static (c, _) => Semantic1Definition.Create(c.SemanticModel, c.TargetNode, c.TargetSymbol)
        );

        var prj = semantic1.Select((d, _) =>
            (FileName: d.GenerationFileName("PrjExtension"), SyntaxNode: d.GeneratePrjExtensionMethod()));
        RegisterSimpleSourceOutput(context, prj);

        var carrierD = semantic1.Select((d, _) =>
            (FileName: d.GenerationFileName("CarrierD"), SyntaxNode: d.DataCarriersSyntax()));
        RegisterSimpleSourceOutput(context, carrierD);

        var carrierB = semantic1.Select((d, _) =>
            (FileName: d.GenerationFileName("CarrierB"), SyntaxNode: d.DataBuilderSyntax()));
        RegisterSimpleSourceOutput(context, carrierB);

        UseSemanticGenerator(context, semantic1, (d) => new Semantic1IdSemanticGenerator(d));
        UseSemanticGenerator(context, semantic1, (d) => new Semantic1ComposeSemanticGenerator(d));
    }


    void UseSemanticGenerator(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<Semantic1Definition> definition,
        Func<Semantic1Definition, ISemanticImplementationGenerator> generatorFactory)
    {
        var output = definition.Select((d, _) =>
        {
            var g = generatorFactory(d);
            return (
                Definition: d,
                g.Category,
                SemanticSyntaxNode: g.ShouldGenerateSemanticUsageBrandMethod()
                    ? g.GenerateSemanticDefinition()
                    : null,
                MethodSyntaxNode: g.ShouldGenerateSemanticUsageBrandMethod()
                    ? g.GenerateSemanticUsageBrandMethod()
                    : null);
        });
        context.RegisterSourceOutput(output, (ctx, data) =>
        {
            if (data.SemanticSyntaxNode is not null)
            {
                ctx.AddSource(
                    data.Definition.GenerationFileName(data.Category),
                    data.Definition.PlaceSameContainerAsSemantic(data.SemanticSyntaxNode)
                        .NormalizeWhitespace()
                        .ToFullString()
                );
            }

            if (data.MethodSyntaxNode is not null)
            {
                ctx.AddSource(
                    data.Definition.GenerationFileName($"{data.Category}-Usage"),
                    data.Definition.PlaceSameContainerAsSemantic(data.MethodSyntaxNode)
                        .NormalizeWhitespace()
                        .ToFullString()
                );
            }
        });
    }

    void RegisterSimpleSourceOutput(IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(string, SyntaxNode)> source)
    {
        context.RegisterSourceOutput(
            source,
            (ctx, data) => ctx.AddSource(data.Item1, data.Item2.NormalizeWhitespace().ToFullString())
        );
    }
}