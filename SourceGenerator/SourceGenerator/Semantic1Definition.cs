using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SemanticAlgebra.SourceGenerator;

public record class Semantic1Definition(
    SemanticModel SemanticModel,
    InterfaceDeclarationSyntax ConcreteSemanticSyntax,
    INamedTypeSymbol ConcreteSemanticSymbol,
    INamedTypeSymbol AbstractSemanticSymbol,
    INamedTypeSymbol BrandSymbol)
{
    public IdentifierNameSyntax TSName { get; } =
        IdentifierName(ConcreteSemanticSyntax.TypeParameterList?.Parameters[^2].Identifier ?? Identifier("TS"));

    public IdentifierNameSyntax TRName { get; } =
        IdentifierName(ConcreteSemanticSyntax.TypeParameterList?.Parameters[^1].Identifier ?? Identifier("TR"));

    public TypeSyntax BrandName { get; } =
        BrandSymbol.Arity == 0
            ? IdentifierName(BrandSymbol.Name)
            : GenericName(Identifier(BrandSymbol.Name))
                .AddTypeArgumentListArguments(
                    [..BrandSymbol.TypeParameters.Select(p => IdentifierName(p.Name))]
                );

    public TypeSyntax ExpressionSyntax()
        => ExpressionSyntax(TSName);


    /// <summary>
    /// Encoding C# syntax for <c>IS&lt;F, t&gt;</c>
    /// </summary>
    /// <param name="t">C# syntax for type argument</param>
    /// <returns></returns>
    public TypeSyntax ExpressionSyntax(TypeSyntax t)
    {
        var semanticAlgebraNamespace = AbstractSemanticSymbol.ContainingNamespace.ToReferenceName();
        return QualifiedName(
            semanticAlgebraNamespace,
            GenericName(
                Identifier("IS"),
                TypeArgumentList().AddArguments(
                    BrandName,
                    t
                )
            )
        );
    }

    public static Semantic1Definition Create(
        SemanticModel semanticModel,
        SyntaxNode targetNode,
        ISymbol targetSymbol)
    {
        var symbol = (INamedTypeSymbol)targetSymbol;
        var abstractSemanticSymbol =
            symbol.Interfaces.Single(i =>
                i is
                {
                    Name: "ISemantic1",
                    ContainingNamespace.Name: "SemanticAlgebra"
                }
            );
        var brandSymbol = abstractSemanticSymbol.TypeArguments[0];
        return new Semantic1Definition(
            semanticModel,
            (InterfaceDeclarationSyntax)targetNode,
            symbol,
            abstractSemanticSymbol,
            (INamedTypeSymbol)brandSymbol
        );
    }

    public string GenerationFileName(string category)
    {
        var bName = BrandSymbol.ToReferenceName();
        var result = $"{bName}-{category}.g.cs";
        result = WebUtility.UrlEncode(result)?.Replace('%', '-') ?? $"{BrandSymbol.Name}.{category}.g.cs";
        return result;
    }

    public SyntaxNode GeneratePrjExtensionMethod()
    {
        var s = Identifier("s");
        var abstractSemantic =
            AbstractSemanticSymbol.ToReferenceName();
        var concreteSemantic =
            ConcreteSemanticSymbol.ToReferenceName();
        var methodTypeParameters = (ConcreteSemanticSyntax.TypeParameterList?.Parameters ?? [])
            .Select(t => t.RemoveVariance());
        var method = MethodDeclaration(concreteSemantic, "Prj")
                     .AddTypeParameterListParameters([
                         ..BrandSymbol.TypeParameters.Select(p => TypeParameter(Identifier(p.Name)))
                     ])
                     .AddTypeParameterListParameters([.. methodTypeParameters])
                     .AddParameterListParameters(
                         Parameter(s)
                             .WithType(abstractSemantic)
                             .AddModifiers(Token(SyntaxKind.ThisKeyword))
                     )
                     .AddConstraintClauses(
                         [
                             ..BrandSymbol.DeclaringSyntaxReferences
                                          .Select(d => (TypeDeclarationSyntax)d.GetSyntax())
                                          .SelectMany(c =>
                                              c.ConstraintClauses.Select(cc =>
                                                  cc.WithConstraints([
                                                      ..cc.Constraints.Select(c_ => c_ switch
                                                      {
                                                          TypeConstraintSyntax ts =>
                                                              SemanticModel.GetSymbolInfo(ts.Type).Symbol is
                                                                  INamedTypeSymbol ss
                                                                  ? TypeConstraint(ss.ToReferenceName())
                                                                  : c_,
                                                          _ => c_
                                                      })
                                                  ])))
                         ]
                     )
                     .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                     .WithExpressionBody(
                         ArrowExpressionClause(
                             CastExpression(
                                 concreteSemantic,
                                 IdentifierName(s)
                             ))
                     )
                     .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        var extCls = ClassDeclaration($"{BrandSymbol.Name}Extensions")
                     .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                     .AddMembers(method);
        return PlaceInSemanticNamespace(extCls);
    }

    public SyntaxNode PlaceInSemanticNamespace(MemberDeclarationSyntax node)
    {
        var ns = ConcreteSemanticSymbol.ContainingNamespace;
        return NamespaceDeclaration(
            ns.ToDefinitionName()
        ).AddMembers(node);
    }

    public SyntaxNode PlaceSameContainerAsSemantic(MemberDeclarationSyntax node)
    {
        return ConcreteSemanticSyntax.ReplaceNestedContainerMemberRecursively(node);
    }


    public SyntaxNode DataCarriersSyntax()
    {
        return ConcreteSemanticSyntax.ReplaceNestedContainerMemberRecursively(
            ClassDeclaration("D")
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword))
                .AddMembers(
                    [
                        ..ConcreteSemanticSymbol.GetMembers()
                                                .OfType<IMethodSymbol>()
                                                .Select(CarrierDefinitionSyntax)
                    ]
                )
        );
    }


    public MemberDeclarationSyntax CarrierDefinitionSyntax(IMethodSymbol symbol)
    {
        var ds = (MethodDeclarationSyntax)symbol.DeclaringSyntaxReferences.Single().GetSyntax();
        var s = IdentifierName("s");
        return ClassDeclaration(symbol.Name)
               .AddModifiers(
                   Token(SyntaxKind.PublicKeyword),
                   Token(SyntaxKind.SealedKeyword),
                   Token(SyntaxKind.RecordKeyword)
               )
               .AddTypeParameterListParameters(TypeParameter(TSName.Identifier))
               .WithParameterList(ds.ParameterList)
               .AddBaseListTypes(SimpleBaseType(ExpressionSyntax()))
               .AddMembers(
                   MethodDeclaration(TRName, "Evaluate")
                       .AddModifiers(Token(SyntaxKind.PublicKeyword))
                       .AddTypeParameterListParameters(TypeParameter(TRName.Identifier))
                       .AddParameterListParameters(
                           Parameter(s.Identifier).WithType(
                               AbstractSemanticSymbol.ToReferenceName())
                       )
                       .WithExpressionBody(
                           ArrowExpressionClause(
                               InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       InvocationExpression(
                                           MemberAccessExpression(
                                               SyntaxKind.SimpleMemberAccessExpression,
                                               s,
                                               IdentifierName("Prj")
                                           )
                                       ),
                                       IdentifierName(symbol.Name)
                                   )).AddArgumentListArguments(
                                   [..ds.ParameterList.Parameters.Select(p => Argument(IdentifierName(p.Identifier)))]
                               )
                           ))
                       .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
               );
    }


    public SyntaxNode DataBuilderSyntax()
    {
        return ConcreteSemanticSyntax.ReplaceNestedContainerMemberRecursively(
            ClassDeclaration("B")
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword))
                .AddMembers(
                    [
                        ..ConcreteSemanticSymbol.GetMembers()
                                                .OfType<IMethodSymbol>()
                                                .Select(CarrierBuilderSyntax)
                    ]
                )
        );
    }

    public MemberDeclarationSyntax CarrierBuilderSyntax(IMethodSymbol symbol)
    {
        var ds = (MethodDeclarationSyntax)symbol.DeclaringSyntaxReferences.Single().GetSyntax();
        return MethodDeclaration(ExpressionSyntax(), symbol.Name)
               .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
               .AddTypeParameterListParameters(TypeParameter(TSName.Identifier))
               .WithParameterList(ds.ParameterList)
               .WithExpressionBody(
                   ArrowExpressionClause(
                       ObjectCreationExpression(
                           QualifiedName(
                               IdentifierName("D"),
                               GenericName(symbol.Name)
                                   .AddTypeArgumentListArguments(TSName)
                           )).AddArgumentListArguments(
                           [..ds.ParameterList.Parameters.Select(p => Argument(IdentifierName(p.Identifier)))]
                       )
                   ))
               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public TypeSyntax ConcreteSemanticName(
        TypeSyntax ts,
        TypeSyntax tr)
    {
        return GenericName(ConcreteSemanticSymbol.Name).AddTypeArgumentListArguments(ts, tr);
    }

    public TypeSyntax AbstractSemanticName(
        TypeSyntax ts,
        TypeSyntax tr
    )
    {
        return QualifiedName(
            AbstractSemanticSymbol.ContainingNamespace.ToReferenceName(),
            GenericName(AbstractSemanticSymbol.Name)
                .AddTypeArgumentListArguments(BrandName, ts, tr)
        );
    }

    public SyntaxNode BrandSemanticUsageSyntax(params ReadOnlySpan<ISemanticImplementationGenerator> semanticGenerators)
    {
        var bs = (TypeDeclarationSyntax)BrandSymbol.DeclaringSyntaxReferences.First().GetSyntax();
        var cls = bs.WithConstraintClauses([])
                    .WithParameterList(null)
                    .WithMembers([]);
        foreach (var generator in semanticGenerators)
        {
            if (generator.ShouldGenerateSemanticUsageBrandMethod())
            {
                cls = cls.AddMembers(generator.GenerateSemanticUsageBrandMethod());
            }
        }

        return bs.ReplaceNestedContainerMemberRecursively(
            cls
        );
    }
}