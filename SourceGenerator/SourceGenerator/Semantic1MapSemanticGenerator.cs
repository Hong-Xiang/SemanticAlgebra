using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SemanticAlgebra.SourceGenerator;

internal sealed class Semantic1MapSemanticGenerator : ISemanticImplementationGenerator
{
    public Semantic1Definition Definition { get; }
    public INamedTypeSymbol ImplementationKind1 { get; }
    public IMethodSymbol ImplementationMethod { get; }

    public Semantic1MapSemanticGenerator(Semantic1Definition definition)
    {
        Definition = definition;
        ImplementationKind1 = Definition.BrandSymbol.AllInterfaces
                                        .Single(t => t.Name == "IFunctor");
        ImplementationMethod = ImplementationKind1.GetMembers()
                                                  .OfType<IMethodSymbol>()
                                                  .Single(m => m.Name == BrandMethodName);
        F = Parameter(Identifier(ImplementationMethod.Parameters[0].Name))
            .WithType(ImplementationMethod.Parameters[0].Type.ToReferenceName());
        TypeParameters = ImplementationMethod.TypeParameters.Select(p => TypeParameter(p.Name)).ToArray();
        TS = IdentifierName(TypeParameters[0].Identifier);
        TR = IdentifierName(TypeParameters[1].Identifier);
    }

    public static string BrandMethodName => "MapS";
    public static string SemanticClassName => "MapSemantic";
    public string Category => SemanticClassName;

    public ParameterSyntax F { get; }
    public TypeParameterSyntax[] TypeParameters { get; }
    public TypeSyntax TS { get; }
    public TypeSyntax TR { get; }

    public ClassDeclarationSyntax GenerateSemanticDefinition()
    {
        return ClassDeclaration(SemanticClassName)
               .AddModifiers(Token(SyntaxKind.SealedKeyword))
               .AddTypeParameterListParameters(TypeParameters)
               .AddBaseListTypes(
                   SimpleBaseType(
                       Definition.ConcreteSemanticName(
                           TS, Definition.ExpressionSyntax(TR)
                       )
                   )
               )
               .AddParameterListParameters(F)
               .AddMembers(
                   [
                       ..Definition.ConcreteSemanticSymbol.GetMembers()
                                   .OfType<IMethodSymbol>()
                                   .Select(MethodDefinitionSyntax)
                   ]
               );
    }

    public MethodDeclarationSyntax GenerateSemanticUsageBrandMethod()
    {
        var k1s = Definition.BrandSymbol.AllInterfaces.Single(x => x.Name == "IFunctor");
        return MethodDeclaration(
                   ImplementationMethod.ReturnType.ToReferenceName(),
                   BrandMethodName)
               .AddModifiers(Token(SyntaxKind.StaticKeyword))
               .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(k1s.ToReferenceName()))
               .AddTypeParameterListParameters(TypeParameters)
               .AddParameterListParameters(F)
               .WithExpressionBody(
                   ArrowExpressionClause(
                       ObjectCreationExpression(
                           QualifiedName(
                               Definition.BrandSymbol.ToReferenceName(),
                               GenericName(SemanticClassName)
                                   .AddTypeArgumentListArguments(
                                       [..TypeParameters.Select(p => IdentifierName(p.Identifier))]
                                   ))
                       ).AddArgumentListArguments(
                           Argument(IdentifierName(F.Identifier)))
                   )
               ).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public bool ShouldGenerateSemanticUsageBrandMethod()
        => Definition.BrandSymbol.AllInterfaces.Any(
            t => t.Name == "IFunctor"
        ) && Definition.BrandSymbol.MemberNames.All(n => n != BrandMethodName);

    MethodDeclarationSyntax MethodDefinitionSyntax(IMethodSymbol symbol)
    {
        var ds = (MethodDeclarationSyntax)symbol.DeclaringSyntaxReferences.Single().GetSyntax();
        var body =
            InvocationExpression(
                    QualifiedName(
                        IdentifierName("B"),
                        GenericName(symbol.Name).AddTypeArgumentListArguments(TR)
                    )
                )
                .AddArgumentListArguments(
                    [
                        ..symbol.Parameters.Select(p =>
                        {
                            if (p.Type.Name == TypeParameters[0].Identifier.ValueText)
                            {
                                return Argument(
                                    InvocationExpression(
                                        IdentifierName(F.Identifier)
                                    ).AddArgumentListArguments(
                                        Argument(IdentifierName(p.Name))));
                            }
                            else
                            {
                                return Argument(IdentifierName(p.Name));
                            }
                        })
                    ]
                );
        return MethodDeclaration(
                   Definition.ExpressionSyntax(TR),
                   symbol.Name)
               .AddModifiers(Token(SyntaxKind.PublicKeyword))
               .WithParameterList(ds.ParameterList)
               .WithExpressionBody(
                   ArrowExpressionClause(body))
               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}