using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SemanticAlgebra.SourceGenerator;

public sealed class ParentNodeHoleVisitor(MemberDeclarationSyntax Hole) : CSharpSyntaxVisitor<SyntaxNode>
{
    public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) =>
        node.WithMembers([Hole])
            .WithAttributeLists([])
            .WithBaseList(null)
            .WithConstraintClauses([])
            .WithoutTrivia();
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) =>
        node.WithMembers([Hole])
            .WithAttributeLists([])
            .WithBaseList(null)
            .WithConstraintClauses([])
            .WithoutTrivia();

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) =>
        node.WithMembers([Hole]).WithoutTrivia();

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node) =>
        node.WithMembers([Hole]).WithoutTrivia();

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node) =>
        node.WithMembers([Hole]).WithoutTrivia();
}