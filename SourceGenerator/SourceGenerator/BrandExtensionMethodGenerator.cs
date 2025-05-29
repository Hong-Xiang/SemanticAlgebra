using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticAlgebra.SourceGenerator;

internal sealed record class BrandExtensionMethodGenerator(
    SemanticModel SemanticModel,
    ISymbol BrandSymbol,
    ISymbol SemanticSymbol,
    ISymbol AbstractionSemantic1Symbol
)
{
    // Generate Code like following in same namespace as brand class
    // static class <BrandClassName>Extension
    // {
    //     public static ISemantic<TS, TR> Prj<TS, TR>(
    //         this <ISemantic1's full name><({brand class full name}, TS, TR> s)
    //         => (ISemantic<TS, TR>)s;
    // }
    public SyntaxNode Generate()
    {
        var brandName = BrandSymbol.Name;
        var brandFullName = BrandSymbol.ToDisplayString();
        var semanticFullName = SemanticSymbol.ToDisplayString();
        
        var extensionClassName = $"{brandName}Extension";
        
        // Use ParseTypeName for complex generic types
        var parameterType = SyntaxFactory.ParseTypeName($"{semanticFullName}<{brandFullName}, TS, TR>");
        var returnType = SyntaxFactory.ParseTypeName("ISemantic<TS, TR>");
        
        // Create the extension method using higher-level factories
        var prjMethod = SyntaxFactory.MethodDeclaration(returnType, "Prj")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddTypeParameterListParameters(SyntaxFactory.TypeParameter("TS"), SyntaxFactory.TypeParameter("TR"))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("s"))
                    .WithType(parameterType)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword))))
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                SyntaxFactory.CastExpression(returnType, SyntaxFactory.IdentifierName("s"))))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        
        // Create the extension class
        var extensionClass = SyntaxFactory.ClassDeclaration(extensionClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddMembers(prjMethod);
        
        return extensionClass;
    }
}
