using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace SemanticAlgebra.SourceGenerator;

public static class SyntaxFactoryExtension
{
    public static SyntaxNode ReplaceContainerMembers(
        this SyntaxNode parent,
        MemberDeclarationSyntax hole
    ) => new ParentNodeHoleVisitor(hole).Visit(parent);

    public static SyntaxNode ReplaceNestedContainerMemberRecursively(
        this MemberDeclarationSyntax source,
        MemberDeclarationSyntax target
    )
    {
        var p = source.Parent;
        SyntaxNode r = target;
        while (p is not null)
        {
            r = p.ReplaceContainerMembers((MemberDeclarationSyntax)r);
            p = p.Parent;
        }

        return r;
    }

    public static TypeParameterSyntax RemoveVariance(this TypeParameterSyntax node)
        => node.WithVarianceKeyword(Token(SyntaxKind.None));

    public static TypeSyntax ToArgument(this TypeParameterSyntax node)
        => IdentifierName(node.Identifier);

    // public static TypeSyntax FullQualifiedName(this ITypeSymbol node)
    //     => node.ContainingType switch
    //     {
    //         null => QualifiedName(node.ContainingNamespace.ToDisplayString(),
    //             IdentifierName(node.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),)
    //     };
    //
    // public static NameSyntax FullQualifiedName(this INamespaceSymbol node)
    //     => node.ContainingNamespace switch
    //     {
    //         null => QualifiedName(IdentifierName(Token(SyntaxKind.GlobalKeyword)), IdentifierName(node.Name)),
    //         var ns => QualifiedName(ns.FullQualifiedName(), IdentifierName(node.Name))
    //     };
    public static NameSyntax ToReferenceName(this INamespaceOrTypeSymbol node)
        => node.Accept(new ReferenceNameVisitor(new NameSettings(true)));

    public static NameSyntax ToDefinitionName(this INamespaceSymbol node)
        => node.Accept(new ReferenceNameVisitor(new NameSettings(false)));
}

sealed record class NameSettings(
    bool PrefixGlobalNamespace
)
{
}

sealed class ReferenceNameVisitor(NameSettings Settings) : SymbolVisitor<NameSyntax>
{
    private NameSyntax CheckNotNull(NameSyntax? name)
        => name ?? throw new InvalidOperationException("result name is null");

    public override NameSyntax? VisitTypeParameter(ITypeParameterSymbol symbol)
        => IdentifierName(symbol.Name);

    public override NameSyntax? VisitNamedType(INamedTypeSymbol symbol)
    {
        SimpleNameSyntax name = symbol.IsGenericType
            ? GenericName(symbol.Name).AddTypeArgumentListArguments(
                [..symbol.TypeArguments.Select(s => CheckNotNull(s.Accept(this)))]
            )
            : IdentifierName(symbol.Name);
        return QualifiedName(symbol.ContainingType switch
        {
            null => CheckNotNull(symbol.ContainingNamespace.Accept(this)),
            var c => CheckNotNull(c.Accept(this))
        }, name);
    }


    public override NameSyntax? VisitNamespace(INamespaceSymbol symbol)
        => symbol switch
        {
            { IsGlobalNamespace: true } =>
                Settings.PrefixGlobalNamespace ? IdentifierName(Token(SyntaxKind.GlobalKeyword)) : null,
            {
                ContainingNamespace:
                {
                    IsGlobalNamespace: true
                } g
            } => CheckNotNull(
                Settings.PrefixGlobalNamespace
                    ? AliasQualifiedName(
                        (IdentifierNameSyntax)CheckNotNull(g.Accept(this)),
                        IdentifierName(symbol.Name)
                    )
                    : IdentifierName(symbol.Name)
            ),
            { ContainingNamespace: var ns } => CheckNotNull(
                QualifiedName(
                    CheckNotNull(ns.Accept(this)),
                    IdentifierName(symbol.Name)
                ))
        };
}