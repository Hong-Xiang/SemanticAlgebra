using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SemanticAlgebra.SourceGenerator;

internal sealed class Semantic1ComposeSemanticGenerator : ISemanticImplementationGenerator
{
    public Semantic1Definition Definition { get; }
    public INamedTypeSymbol ImplementationKind1 { get; }
    public IMethodSymbol ImplementationMethod { get; }

    public Semantic1ComposeSemanticGenerator(Semantic1Definition definition)
    {
        Definition = definition;
        ImplementationKind1 = Definition.BrandSymbol.AllInterfaces
                                        .Single(t => t.Name == "IKind1");
        ImplementationMethod = ImplementationKind1.GetMembers()
                                                  .OfType<IMethodSymbol>()
                                                  .Single(m => m.Name == BrandMethodName);
        S = Parameter(
            Identifier(ImplementationMethod.Parameters[0].Name)
        ).WithType(Definition.ConcreteSemanticName(
            IdentifierName(ImplementationMethod.TypeArguments[0].Name),
            IdentifierName(ImplementationMethod.TypeArguments[1].Name)
        ));
        F = Parameter(Identifier(ImplementationMethod.Parameters[1].Name))
            .WithType(ImplementationMethod.Parameters[1].Type.ToReferenceName());
        TypeParameters = ImplementationMethod.TypeParameters.Select(p => TypeParameter(p.Name)).ToArray();
        TS = IdentifierName(TypeParameters[0].Identifier);
        TI = IdentifierName(TypeParameters[1].Identifier);
        TR = IdentifierName(TypeParameters[2].Identifier);
    }

    public static string BrandMethodName => "Compose";
    public static string SemanticClassName => "ComposeSemantic";
    public string Category => SemanticClassName;

    public ParameterSyntax S { get; }
    public ParameterSyntax F { get; }
    public TypeParameterSyntax[] TypeParameters { get; }
    public TypeSyntax TS { get; }
    public TypeSyntax TI { get; }
    public TypeSyntax TR { get; }

    public ClassDeclarationSyntax GenerateSemanticDefinition()
    {
        return ClassDeclaration(SemanticClassName)
               .AddModifiers(Token(SyntaxKind.SealedKeyword))
               .AddTypeParameterListParameters(TypeParameters)
               .AddBaseListTypes(
                   SimpleBaseType(
                       Definition.ConcreteSemanticName(
                           TS, TR
                       )
                   )
               )
               .AddParameterListParameters(S, F)
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
        var k1s = Definition.BrandSymbol.AllInterfaces.Single(x => x.Name == "IKind1");
        return MethodDeclaration(
                   ImplementationMethod.ReturnType.ToReferenceName(),
                   BrandMethodName)
               .AddModifiers(Token(SyntaxKind.StaticKeyword))
               .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(k1s.ToReferenceName()))
               .AddTypeParameterListParameters(TypeParameters)
               .AddParameterListParameters(
                   S.WithType(ImplementationMethod.Parameters[0].Type.ToReferenceName())
                 , F)
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
                           Argument(
                               InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       IdentifierName(S.Identifier),
                                       IdentifierName("Prj")
                                   ))
                           ),
                           Argument(IdentifierName(F.Identifier)))
                   )
               ).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public bool ShouldGenerateSemanticUsageBrandMethod()
        => Definition.BrandSymbol.MemberNames.All(n => n != BrandMethodName);

    MethodDeclarationSyntax MethodDefinitionSyntax(IMethodSymbol symbol)
    {
        var ds = (MethodDeclarationSyntax)symbol.DeclaringSyntaxReferences.Single().GetSyntax();
        var callS = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(S.Identifier),
                IdentifierName(symbol.Name))
        ).AddArgumentListArguments(
            [
                ..ds.ParameterList.Parameters.Select(p =>
                    Argument(IdentifierName(p.Identifier)))
            ]
        );
        var callF = InvocationExpression(IdentifierName(F.Identifier))
            .AddArgumentListArguments(
                Argument(callS));
        return MethodDeclaration(
                   IdentifierName(TypeParameters[2].Identifier),
                   symbol.Name)
               .AddModifiers(Token(SyntaxKind.PublicKeyword))
               .WithParameterList(ds.ParameterList)
               .WithExpressionBody(
                   ArrowExpressionClause(callF))
               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}