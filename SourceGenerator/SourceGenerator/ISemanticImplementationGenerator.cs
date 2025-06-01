using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SemanticAlgebra.SourceGenerator;

public interface ISemanticImplementationGenerator
{
    public string Category { get; }
    public ClassDeclarationSyntax GenerateSemanticDefinition();
    public MethodDeclarationSyntax GenerateSemanticUsageBrandMethod();
    public bool ShouldGenerateSemanticUsageBrandMethod();
}