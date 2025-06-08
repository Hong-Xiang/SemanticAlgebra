using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SemanticAlgebra;
using SemanticAlgebra.SourceGenerator;
using System.Net;
using Xunit.Abstractions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGeneratorCoreTest;

public class SyntaxDrivenGenerationTest(ITestOutputHelper Output)
{
    [Fact]
    public Task ToReferenceNameGenericTest()
    {
        var compilation = CSharpCompilation.Create(
            "Test.dll",
            [
                CSharpSyntaxTree.ParseText("""
                                           namespace TNS;
                                           class C<T1, T2> {
                                              class T<T3> {
                                              }
                                           }
                                           """)
            ],
            [
                ..ReferenceAssemblies.Net80,
            ]
        );
        var tree = Assert.Single(compilation.SyntaxTrees);
        var root = tree.GetRoot();
        var target = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                         .Single(s => s.Identifier.ValueText == "T");
        var semanticModel = compilation.GetSemanticModel(tree);
        var symbol = semanticModel.GetDeclaredSymbol(target)!;
        var result = symbol.ToReferenceName().NormalizeWhitespace().ToFullString();
        Output.WriteLine(result);
        return Verify(result);
    }

    [Fact]
    public void GetSymbolTest()
    {
        var compilation = CSharpCompilation.Create(
            "Test.dll",
            [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
            [
                ..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );

        var tree = Assert.Single(compilation.SyntaxTrees);
        var root = tree.GetRoot();
        var semanticModel = compilation.GetSemanticModel(tree);
        var semanticSyntax = root
                             .DescendantNodes(_ => true)
                             .OfType<InterfaceDeclarationSyntax>()
                             .Single(s => s.Identifier.ValueText == "ISemantic");
        var symbol = semanticModel.GetDeclaredSymbol(semanticSyntax)!;
        var result = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        Output.WriteLine(result);
        result = WebUtility.UrlEncode(result).Replace('%', '.');
        Output.WriteLine(result);
    }


    [Fact]
    public void QualifiedNameTest()
    {
        var name = QualifiedName(QualifiedName(IdentifierName("A"), IdentifierName("B")), IdentifierName("C"));
        Output.WriteLine(name.ToFullString());
    }

    [Fact]
    public Task ReplaceNodeWithOriginalContextTest()
    {
        var compilation = CSharpCompilation.Create(
            "Test.dll",
            [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
            [
                ..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );

        var tree = Assert.Single(compilation.SyntaxTrees);
        var root = tree.GetRoot();
        var source = root.DescendantNodes(static _ => true)
                         .OfType<InterfaceDeclarationSyntax>()
                         .Where(static s => s.Identifier.ValueText == "ISemantic")
                         .Single();
        var target = MethodDeclaration(IdentifierName("int"), "Bar")
                     .WithExpressionBody(
                         ArrowExpressionClause(
                             LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                 Literal(42))))
                     .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var r = source.ReplaceNestedContainerMemberRecursively(target);
        var resultCode = r.NormalizeWhitespace().ToFullString();
        return Verify(resultCode);
    }

    [Fact]
    public void SemanticAlgebraISemantic1Test()
    {
        var name = SemanticAlgebraAbstraction.ISemantic1(
            IdentifierName("Option"),
            IdentifierName("TS"),
            IdentifierName("TR"));
        Assert.Equal("SemanticAlgebra.ISemantic1<Option,TS,TR>", name.ToFullString());
        Output.WriteLine(name.ToFullString());
    }

    [Fact]
    public void GetKind1MethodTest()
    {
        var compilation = CSharpCompilation.Create(
            "Test.dll",
            [CSharpSyntaxTree.ParseText(Utility.OptionBrandCode, path: "Option.cs")],
            [
                ..ReferenceAssemblies.Net80,
                MetadataReference.CreateFromFile(typeof(IKind1<>).Assembly.Location),
            ]
        );

        var tree = Assert.Single(compilation.SyntaxTrees);
        var root = tree.GetRoot();
        var source = root.DescendantNodes(static _ => true)
                         .OfType<ClassDeclarationSyntax>()
                         .Where(static s => s.Identifier.ValueText == "Option")
                         .Single();
        var semanticModel = compilation.GetSemanticModel(tree);
        var symbol = semanticModel.GetDeclaredSymbol(source)!;
        var interfaces = symbol.AllInterfaces;
        foreach (var t in interfaces)
        {
            Output.WriteLine($"{t.Name}:{t.GetType().Name}");
            if (t.Name == "IKind1")
            {
                foreach (var m in t.GetMembers())
                {
                    Output.WriteLine(new string('-', 20));
                    Output.WriteLine($"{m.Name}:{m.Name}");
                    if (m is IMethodSymbol ms)
                    {
                        foreach (var p in ms.Parameters)
                        {
                            Output.WriteLine($"{p.Name}:{p.Type.ToReferenceName().ToFullString()}");
                        }

                        Output.WriteLine(" -> ");
                        Output.WriteLine(ms.ReturnType.ToReferenceName().ToFullString());
                    }

                    Output.WriteLine(new string('-', 20));
                }
            }
        }
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