using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SemanticAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGeneratorCoreTest;

public class SyntaxDrivenGenerationTest(ITestOutputHelper Output)
{
    sealed class PrintVisitor(SemanticModel SemanticModel, ITestOutputHelper Output) : CSharpSyntaxWalker
    {
        bool visitingSemanticInterface = false;
        Stack<SyntaxNode> GeneratedNodes = [];
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            Output.WriteLine(node.Identifier.ValueText);
            base.VisitMethodDeclaration(node);
        }
        public override void VisitTypeParameter(TypeParameterSyntax node)
        {
            Output.WriteLine($"variance of {node} = {node.VarianceKeyword}");
            base.VisitTypeParameter(node);
        }
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (node.Identifier.ValueText == "ISemantic")
            {
                visitingSemanticInterface = true;
                {
                    var argumentName = Identifier("s");
                    var typeParameters = node
                        .TypeParameterList
                        ?.RemoveTypeParameterVariance();
                    var typeArguments = node.TypeParameterList?.ToTypeArgumentList() ?? TypeArgumentList();
                    var semanticTypeName = GenericName(node.Identifier, typeArguments);
                    var method = MethodDeclaration(
                        semanticTypeName,
                        "Prj"
                    ).WithTypeParameterList(typeParameters)
                     .AddParameterListParameters(
                        Parameter(argumentName)
                        .WithType(
                            GenericName("ISemantic1")
                            .WithTypeArgumentList(typeArguments)
                        )
                     )
                     .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                     .WithExpressionBody(
                        ArrowExpressionClause(
                            CastExpression(
                                semanticTypeName,
                                IdentifierName(argumentName)
                            ))
                        );

                    Output.WriteLine(method.NormalizeWhitespace().ToFullString());
                }
                visitingSemanticInterface = false;
            }
            base.VisitInterfaceDeclaration(node);
        }
    }

    [Fact]
    public void PrjMethodShouldTranslate()
    {
        var compilation = CSharpCompilation.Create(
            "Test.dll",
            [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
            [..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );

        var tree = Assert.Single(compilation.SyntaxTrees);
        var root = tree.GetRoot();
        var visitor = new PrintVisitor(compilation.GetSemanticModel(tree), Output);
        visitor.Visit(root);
    }
}

sealed class TypeParamterListToTypeArgumentListRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitTypeParameter(TypeParameterSyntax node)
    {
        return IdentifierName(node.Identifier);
    }
    public override SyntaxNode? VisitTypeParameterList(TypeParameterListSyntax node)
    {
        var children = node.ChildNodes().Select(t => (TypeSyntax)Visit(t));
        return TypeArgumentList().AddArguments([.. children]);
    }
}

sealed class TypeParameterRemoveVarianceRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitTypeParameter(TypeParameterSyntax node)
    {
        return base.VisitTypeParameter(node.Update(
            node.AttributeLists,
            Token(SyntaxKind.None),
            node.Identifier));
    }
}

static class SyntaxNodeExtension
{
    public static TypeParameterListSyntax RemoveTypeParameterVariance(this TypeParameterListSyntax node)
        => (TypeParameterListSyntax)new TypeParameterRemoveVarianceRewriter().Visit(node);
    public static TypeArgumentListSyntax ToTypeArgumentList(this TypeParameterListSyntax node)
        => (TypeArgumentListSyntax)new TypeParamterListToTypeArgumentListRewriter().Visit(node);
}
