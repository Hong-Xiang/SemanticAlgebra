using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SemanticAlgebra.SourceGenerator;

internal sealed class Semantic1IdSemanticGenerator(
    Semantic1Definition Definition
) : ISemanticImplementationGenerator
{
    public static string SemanticClassName => "IdSemantic";
    public static string BrandMethodName => "Id";


    public string Category => SemanticClassName;

    public ClassDeclarationSyntax GenerateSemanticDefinition()
    {
        return ClassDeclaration(SemanticClassName)
               .AddModifiers(Token(SyntaxKind.SealedKeyword))
               .AddTypeParameterListParameters(TypeParameter(Definition.TSName.Identifier))
               .AddBaseListTypes(
                   SimpleBaseType(
                       Definition.ConcreteSemanticName(Definition.TSName, Definition.ExpressionSyntax())
                   )
               )
               .AddMembers(
                   [
                       ..Definition.ConcreteSemanticSymbol.GetMembers()
                                   .OfType<IMethodSymbol>()
                                   .Select(IdSemanticMethodSyntax)
                   ]
               );
    }

    public MethodDeclarationSyntax GenerateSemanticUsageBrandMethod()
    {
        var k1s = Definition.BrandSymbol.AllInterfaces.Single(x => x.Name == "IKind1");
        return MethodDeclaration(
                   Definition.AbstractSemanticName(
                       Definition.TSName, Definition.ExpressionSyntax()), BrandMethodName)
               .AddModifiers(Token(SyntaxKind.StaticKeyword))
               .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(k1s.ToReferenceName()))
               .AddTypeParameterListParameters(TypeParameter(Definition.TSName.Identifier))
               .WithExpressionBody(
                   ArrowExpressionClause(
                       ObjectCreationExpression(
                           QualifiedName(
                               Definition.BrandSymbol.ToReferenceName(),
                               GenericName(SemanticClassName)
                                   .AddTypeArgumentListArguments(Definition.TSName))
                       ).AddArgumentListArguments()
                   )
               ).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public bool ShouldGenerateSemanticUsageBrandMethod()
        => Definition.BrandSymbol.MemberNames.All(n => n != BrandMethodName);

    MethodDeclarationSyntax IdSemanticMethodSyntax(IMethodSymbol symbol)
    {
        var ds = (MethodDeclarationSyntax)symbol.DeclaringSyntaxReferences.Single().GetSyntax();
        return MethodDeclaration(Definition.ExpressionSyntax(), symbol.Name)
               .AddModifiers(Token(SyntaxKind.PublicKeyword))
               // .AddTypeParameterListParameters(TypeParameter(Definition.TSName.Identifier))
               .WithParameterList(ds.ParameterList)
               .WithExpressionBody(
                   ArrowExpressionClause(
                       InvocationExpression(
                           QualifiedName(
                               IdentifierName("B"),
                               GenericName(symbol.Name)
                                   .AddTypeArgumentListArguments(Definition.TSName)
                           )).AddArgumentListArguments(
                           [
                               ..ds.ParameterList.Parameters.Select(p =>
                                   Argument(IdentifierName(p.Identifier)))
                           ]
                       )
                   ))
               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}