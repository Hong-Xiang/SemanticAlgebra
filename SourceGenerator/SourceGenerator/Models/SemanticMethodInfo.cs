using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace SemanticAlgebra.SourceGenerator.Models;

/// <summary>
/// Information about a method in an ISemantic interface that needs code generation
/// </summary>
public sealed record SemanticMethodInfo(
    string MethodName,
    IReadOnlyList<SemanticParameterInfo> Parameters,
    string ReturnTypeName,
    bool HasGenericTypeParameter)
{
    /// <summary>
    /// Gets the generic type parameter name if this method has one, typically 'T'
    /// </summary>
    public string? GenericTypeParameter => HasGenericTypeParameter ? "T" : null;
    
    /// <summary>
    /// Gets the builder method signature
    /// </summary>
    public string GetBuilderSignature(string className) => 
        $"public static IS<{className}, {GenericTypeParameter ?? "T"}> {MethodName}" +
        (HasGenericTypeParameter ? "<T>" : "") +
        $"({string.Join(", ", Parameters.Select(p => p.GetBuilderParameterDeclaration()))})";
    
    /// <summary>
    /// Gets the data record signature
    /// </summary>
    public string GetDataRecordSignature(string className) =>
        $"public sealed record class {MethodName}" +
        (HasGenericTypeParameter ? "<T>" : "") +
        $"({string.Join(", ", Parameters.Select(p => p.GetRecordParameterDeclaration()))}) : IS<{className}, {GenericTypeParameter ?? "T"}>";
}

/// <summary>
/// Information about a parameter in a semantic method
/// </summary>
public sealed record SemanticParameterInfo(
    string Name,
    string TypeName,
    bool IsGenericCarrierType)
{
    /// <summary>
    /// Gets the parameter declaration for a builder method
    /// </summary>
    public string GetBuilderParameterDeclaration() => 
        IsGenericCarrierType ? $"T {Name.ToLowerInvariant()}" : $"{TypeName} {Name.ToLowerInvariant()}";
    
    /// <summary>
    /// Gets the parameter declaration for a record class
    /// </summary>
    public string GetRecordParameterDeclaration() =>
        IsGenericCarrierType ? $"T {Name}" : $"{TypeName} {Name}";
        
    /// <summary>
    /// Gets the argument for calling the semantic method
    /// </summary>
    public string GetSemanticArgument() => Name;
}
