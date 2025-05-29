using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SourceGeneratorCore;

interface IAttributeSyntaxNodeGeneratorSource<T>
{
    string AttributeName { get; }
    bool IsTarget(SyntaxNode node, CancellationToken cancellation);
    T Transform(GeneratorAttributeSyntaxContext context);
    IncrementalValuesProvider<ISourceGenerationResult> Process(IncrementalValuesProvider<SyntaxNodeContext<T>> source);
}

public readonly record struct SyntaxNodeContext<T>(
    T TargetNode,
    ISymbol TargetSymbol,
    SemanticModel SemanticModel
)
{
}

public sealed class Kind1BrandClassDeclSource
    : IAttributeSyntaxNodeGeneratorSource<SyntaxNodeContext<TypeDeclarationSyntax>>
{
    public string AttributeName => "SemanticAlgebra.Syntax.SemanticKind1BrandAttribute";

    public bool IsTarget(SyntaxNode node, CancellationToken cancellation)
        => node is ClassDeclarationSyntax or InterfaceDeclarationSyntax;

    public IncrementalValuesProvider<ISourceGenerationResult> Process(IncrementalValuesProvider<SyntaxNodeContext<SyntaxNodeContext<TypeDeclarationSyntax>>> source)
    {
        throw new NotImplementedException();
    }

    public SyntaxNodeContext<TypeDeclarationSyntax> Transform(GeneratorAttributeSyntaxContext context)
        => new((TypeDeclarationSyntax)context.TargetNode, context.TargetSymbol, context.SemanticModel);
}
