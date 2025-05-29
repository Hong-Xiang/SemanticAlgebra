using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace SemanticAlgebra.SourceGenerator.Models;

/// <summary>
/// Complete information about a semantic kind for source generation
/// </summary>
public sealed record SemanticKindInfo(
    string ClassName,
    string? Namespace,
    string? SemanticInterfaceName,
    bool HasISemantic,
    IReadOnlyList<SemanticMethodInfo> SemanticMethods,
    Location Location)
{
    /// <summary>
    /// Gets the full qualified semantic interface name
    /// </summary>
    public string FullSemanticInterfaceName => $"{ClassName}.{SemanticInterfaceName}";
    
    /// <summary>
    /// Gets whether this semantic kind has any methods to generate code for
    /// </summary>
    public bool HasMethodsToGenerate => SemanticMethods.Count > 0;
}

/// <summary>
/// Information about generated code for a single semantic case
/// </summary>
public sealed record SemanticCaseGenerationInfo(
    SemanticKindInfo KindInfo,
    SemanticMethodInfo MethodInfo)
{
    /// <summary>
    /// Gets the builder method source code
    /// </summary>
    public string GetBuilderMethodSource() =>
        $"        {MethodInfo.GetBuilderSignature(KindInfo.ClassName)} => new D.{MethodInfo.MethodName}" +
        (MethodInfo.HasGenericTypeParameter ? "<T>" : "") +
        $"({string.Join(", ", MethodInfo.Parameters.Select(p => p.Name.ToLowerInvariant()))});";
    
    /// <summary>
    /// Gets the data definition source code
    /// </summary>
    public string GetDataDefinitionSource() =>
        $@"        {MethodInfo.GetDataRecordSignature(KindInfo.ClassName)}
        {{
            public TR Evaluate<TR>(ISemantic1<{KindInfo.ClassName}, {MethodInfo.GenericTypeParameter ?? "T"}, TR> semantic)
                => semantic.Prj().{MethodInfo.MethodName}({string.Join(", ", MethodInfo.Parameters.Select(p => p.GetSemanticArgument()))});
        }}";
}
