using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace SemanticAlgebra.SourceGenerator;

[Generator]
public sealed class SemanticKindSourceGenerator : IIncrementalGenerator
{
    private const string BrandAttributeFullQualifiedName = "SemanticAlgebra.Syntax.SemanticKind1BrandAttribute";

    // Diagnostic descriptors for logging
    private static readonly DiagnosticDescriptor InfoDescriptor = new DiagnosticDescriptor(
        id: "SGEN001",
        title: "Source Generator Info",
        messageFormat: "SemanticKind Source Generator: {0}",
        category: "SourceGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DebugDescriptor = new DiagnosticDescriptor(
        id: "SGEN002",
        title: "Source Generator Debug",
        messageFormat: "SemanticKind Source Generator Debug: {0}",
        category: "SourceGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a provider for classes with SemanticKind1Brand attribute
        var brandClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BrandAttributeFullQualifiedName,
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .WithTrackingName("BrandClasses");

        // Combine and generate sources
        context.RegisterSourceOutput(brandClasses,
            static (spc, source) => Execute(source, spc));
    }

    /// <summary>
    /// Container for all information needed to generate code for a semantic kind
    /// </summary>
    private sealed record class SemanticKindInfo(
        string ClassName,
        string? Namespace,
        string? SemanticInterfaceName,
        bool HasISemantic);    /// <summary>
    /// Gets the semantic target for generation if it matches our criteria
    /// </summary>
    private static SemanticKindInfo GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        var className = classDeclaration.Identifier.ValueText;
        var namespaceName = GetNamespace(classDeclaration);

        // Look for ISemantic interface within the class
        var semanticInterface = FindISemanticInterface(classDeclaration);
        
        return new SemanticKindInfo(
            className,
            namespaceName,
            semanticInterface?.Identifier.ValueText,
            semanticInterface != null);
    }

    /// <summary>
    /// Finds the ISemantic interface within a class declaration
    /// </summary>
    private static InterfaceDeclarationSyntax? FindISemanticInterface(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Members
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => i.Identifier.ValueText == "ISemantic");
    }    /// <summary>
     /// Executes the source generation for the matched classes
     /// </summary>
    private static void Execute(SemanticKindInfo semanticInfo, SourceProductionContext context)
    {
        // Log execution start
        ReportInfo(context, $"Starting source generation for: {semanticInfo.ClassName}");

        // Generate basic partial class (existing functionality)
        GeneratePartialClass(context, semanticInfo);

        // Generate Prj extension method if ISemantic interface is found
        if (semanticInfo.HasISemantic)
        {
            ReportDebug(context, $"Generating Prj extension method for: {semanticInfo.ClassName}");
            GeneratePrjExtensionMethod(context, semanticInfo);
        }

        ReportInfo(context, "Source generation completed successfully.");
    }    /// <summary>
    /// Generates a partial class with the SourceGenTest property
    /// </summary>
    private static void GeneratePartialClass(SourceProductionContext context, SemanticKindInfo semanticInfo)
    {
        var source = GenerateSourceCode(semanticInfo.Namespace, semanticInfo.ClassName);
        context.AddSource($"{semanticInfo.ClassName}.SourceGenTest.g.cs", source);
    }

    /// <summary>
    /// Generates the Prj extension method for the semantic interface
    /// </summary>
    private static void GeneratePrjExtensionMethod(SourceProductionContext context, SemanticKindInfo semanticInfo)
    {
        if (!semanticInfo.HasISemantic || semanticInfo.SemanticInterfaceName == null)
            return;

        var source = GeneratePrjExtensionCode(semanticInfo.Namespace, semanticInfo.ClassName);
        context.AddSource($"{semanticInfo.ClassName}Extension.Prj.g.cs", source);
    }    /// <summary>
    /// Gets the namespace of a class declaration
    /// </summary>
    private static string? GetNamespace(SyntaxNode classDeclaration)
    {
        // Try to get namespace from namespace declaration
        var namespaceDeclaration = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDeclaration != null)
        {
            return namespaceDeclaration.Name.ToString();
        }

        // Try to get from file-scoped namespace
        var fileScopedNamespace = classDeclaration.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
        if (fileScopedNamespace != null)
        {
            return fileScopedNamespace.Name.ToString();
        }

        return null;
    }

    /// <summary>
    /// Generates the source code for the partial class
    /// </summary>
    private static string GenerateSourceCode(string? namespaceName, string className)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName))
        {
            sb.AppendLine($"namespace {namespaceName};");
            sb.AppendLine();
        }

        sb.AppendLine($"partial class {className}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Auto-generated property by SemanticAlgebra Source Generator");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public string SourceGenTest { get; } = \"TestGen\";");
        sb.AppendLine("    public string SourceGenTest2 { get; } = \"TestGen\";");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the source code for the Prj extension method
    /// </summary>
    private static string GeneratePrjExtensionCode(string? namespaceName, string className)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using SemanticAlgebra;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName))
        {
            sb.AppendLine($"namespace {namespaceName};");
            sb.AppendLine();
        }

        sb.AppendLine($"public static partial class {className}Extension");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Auto-generated Prj extension method for semantic projection");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static {className}.ISemantic<TS, TR> Prj<TS, TR>(");
        sb.AppendLine($"        this ISemantic1<{className}, TS, TR> semantic");
        sb.AppendLine($"    ) => ({className}.ISemantic<TS, TR>)semantic;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // Helper methods for logging to compiler output
    private static void ReportInfo(SourceProductionContext context, string message)
    {
        var diagnostic = Diagnostic.Create(InfoDescriptor, Location.None, message);
        context.ReportDiagnostic(diagnostic);
    }

    private static void ReportDebug(SourceProductionContext context, string message)
    {
        var diagnostic = Diagnostic.Create(DebugDescriptor, Location.None, message);
        context.ReportDiagnostic(diagnostic);
    }
}
