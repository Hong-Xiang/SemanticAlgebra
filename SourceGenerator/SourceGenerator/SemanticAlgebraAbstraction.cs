using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SemanticAlgebra.SourceGenerator;

public sealed class SemanticAlgebraAbstraction
{
    public static string Semantic1AttributeName => "SemanticAlgebra.Syntax.Semantic1Attribute";
    public static SemanticAlgebraAbstraction Instance { get; } = new();

    public static NameSyntax Namespace { get; }
        = IdentifierName("SemanticAlgebra");

    public static NameSyntax ISemantic1(TypeSyntax b, params ReadOnlySpan<TypeSyntax> targs)
        => QualifiedName(
            Namespace,
            GenericName("ISemantic1")
                .AddTypeArgumentListArguments([b, .. targs]));
}

public sealed class SemanticAlgebraSyntaxAbstraction(INamedTypeSymbol concreteSemanticSymbol)
{
    public INamedTypeSymbol AbstractSemanticSymbol { get; } = concreteSemanticSymbol.OriginalDefinition;
    public INamedTypeSymbol Kind1 { get; } = GetIKind1(concreteSemanticSymbol.OriginalDefinition);

    static INamedTypeSymbol GetIKind1(INamedTypeSymbol semantic)
        => (INamedTypeSymbol)semantic.TypeParameters[0].ConstraintTypes[0].OriginalDefinition;
}