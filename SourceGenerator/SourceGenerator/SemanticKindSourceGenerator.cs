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

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: Source provider - Find classes with SemanticKind1Brand attribute and ISemantic interface
        var sourceProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BrandAttributeFullQualifiedName,
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetSemanticKindContext(ctx))
            .Where(static ctx => ctx is not null)
            .Select(static (ctx, _) => ctx!)
            .WithTrackingName("SemanticKindContexts");


        // Step 2: Generate Prj extension method (existing functionality)
        var prjMethodProvider = sourceProvider
            .WithTrackingName("PrjMethods");

        context.RegisterSourceOutput(prjMethodProvider,
            static (spc, ctx) => GeneratePrjExtensionMethod(spc, ctx));

        // Step 3: Cases provider - Extract methods from ISemantic interface (each method = one case)
        var casesProvider = sourceProvider
            .SelectMany(static (ctx, _) => ctx?.Cases ?? ImmutableArray<SemanticCaseInfo>.Empty)
            .WithTrackingName("SemanticCases");

        // Step 4: Definitions provider - Generate D class constructors  
        var definitionsProvider = sourceProvider
            .Select(static (ctx, _) => ctx!)
            .WithTrackingName("DataDefinitions");

        context.RegisterSourceOutput(definitionsProvider,
            static (spc, ctx) => GenerateDataDefinitions(spc, ctx));

        // Step 5: Builders provider - Generate B class methods
        var buildersProvider = sourceProvider
            .Select(static (ctx, _) => ctx!)
            .WithTrackingName("Builders");

        context.RegisterSourceOutput(buildersProvider,
            static (spc, ctx) => GenerateBuilders(spc, ctx));
    }

    /// <summary>
    /// Container for all information needed to generate code for a semantic kind
    /// </summary>
    private sealed record class SemanticKindContext(
        string ClassName,
        string? Namespace,
        InterfaceDeclarationSyntax? SemanticInterface,
        ImmutableArray<SemanticCaseInfo> Cases);

    /// <summary>
    /// Information about a single case (method) in the ISemantic interface
    /// </summary>
    private sealed record class SemanticCaseInfo(
        string MethodName,
        ImmutableArray<SemanticParameterInfo> Parameters,
        string ReturnType);

    /// <summary>
    /// Information about a parameter in a semantic method
    /// </summary>
    private sealed record class SemanticParameterInfo(
        string Name,
        string Type);

    /// <summary>
    /// Gets the semantic kind context for generation if it matches our criteria
    /// </summary>
    private static SemanticKindContext? GetSemanticKindContext(GeneratorAttributeSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        var className = classDeclaration.Identifier.ValueText;
        var namespaceName = GetNamespace(classDeclaration);

        // Look for ISemantic interface within the class
        var semanticInterface = FindISemanticInterface(classDeclaration);

        if (semanticInterface == null)
        {
            return null;
        }

        // Extract cases (methods) from the ISemantic interface
        var cases = ExtractSemanticCases(semanticInterface);

        return new SemanticKindContext(
            className,
            namespaceName,
            semanticInterface,
            cases);
    }

    /// <summary>
    /// Finds the ISemantic interface within a class declaration
    /// </summary>
    private static InterfaceDeclarationSyntax? FindISemanticInterface(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Members
            .OfType<InterfaceDeclarationSyntax>()
            .FirstOrDefault(i => i.Identifier.ValueText == "ISemantic");
    }

    /// <summary>
    /// Extracts semantic cases (methods) from the ISemantic interface
    /// </summary>
    private static ImmutableArray<SemanticCaseInfo> ExtractSemanticCases(InterfaceDeclarationSyntax semanticInterface)
    {
        var cases = ImmutableArray.CreateBuilder<SemanticCaseInfo>();

        foreach (var member in semanticInterface.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodName = member.Identifier.ValueText;
            var returnType = member.ReturnType.ToString();

            var parameters = member.ParameterList.Parameters
                .Select(p => new SemanticParameterInfo(
                    p.Identifier.ValueText,
                    p.Type?.ToString() ?? "object"))
                .ToImmutableArray();

            cases.Add(new SemanticCaseInfo(methodName, parameters, returnType));
        }

        return cases.ToImmutable();
    }

    /// <summary>
    /// Generates the Prj extension method for the semantic interface
    /// </summary>
    private static void GeneratePrjExtensionMethod(SourceProductionContext context, SemanticKindContext semanticContext)
    {
        var source = GeneratePrjExtensionCode(semanticContext.Namespace, semanticContext.ClassName);
        context.AddSource($"{semanticContext.ClassName}Extension.Prj.g.cs", source);
    }

    /// <summary>
    /// Generates the data definitions (D class) for the semantic kind
    /// </summary>
    private static void GenerateDataDefinitions(SourceProductionContext context, SemanticKindContext semanticContext)
    {
        var source = GenerateDataDefinitionsCode(semanticContext);
        context.AddSource($"{semanticContext.ClassName}.DataDefinitions.g.cs", source);
    }

    /// <summary>
    /// Generates the builders (B class) for the semantic kind
    /// </summary>
    private static void GenerateBuilders(SourceProductionContext context, SemanticKindContext semanticContext)
    {
        var source = GenerateBuildersCode(semanticContext);
        context.AddSource($"{semanticContext.ClassName}.Builders.g.cs", source);
    }

    /// <summary>
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

    /// <summary>
    /// Generates the source code for the data definitions (D class)
    /// </summary>
    private static string GenerateDataDefinitionsCode(SemanticKindContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using SemanticAlgebra;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(context.Namespace))
        {
            sb.AppendLine($"namespace {context.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"public sealed partial class {context.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Auto-generated data definitions");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static partial class D");
        sb.AppendLine("    {");

        foreach (var case_ in context.Cases)
        {
            GenerateDataConstructor(sb, context.ClassName, case_);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a single data constructor
    /// </summary>
    private static void GenerateDataConstructor(StringBuilder sb, string className, SemanticCaseInfo case_)
    {
        var constructorName = case_.MethodName;
        var hasParameters = case_.Parameters.Length > 0;

        sb.AppendLine($"        public sealed record class {constructorName}<T>(");

        if (hasParameters)
        {
            for (int i = 0; i < case_.Parameters.Length; i++)
            {
                var param = case_.Parameters[i];
                var paramType = ConvertSemanticTypeToConstructorType(param.Type);
                var comma = i < case_.Parameters.Length - 1 ? "," : "";
                sb.AppendLine($"            {paramType} {CapitalizeFirstLetter(param.Name)}{comma}");
            }
        }

        sb.AppendLine($"        ) : IS<{className}, T>");
        sb.AppendLine("        {");
        sb.AppendLine("            public TR Evaluate<TR>(ISemantic1<" + className + ", T, TR> semantic)");

        if (hasParameters)
        {
            var args = string.Join(", ", case_.Parameters.Select(p => CapitalizeFirstLetter(p.Name)));
            sb.AppendLine($"                => semantic.Prj().{constructorName}({args});");
        }
        else
        {
            sb.AppendLine($"                => semantic.Prj().{constructorName}();");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
    }

    /// <summary>
    /// Generates the source code for the builders (B class)
    /// </summary>
    private static string GenerateBuildersCode(SemanticKindContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using SemanticAlgebra;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(context.Namespace))
        {
            sb.AppendLine($"namespace {context.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"public sealed partial class {context.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Auto-generated data value builders");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static partial class B");
        sb.AppendLine("    {");

        foreach (var case_ in context.Cases)
        {
            GenerateBuilderMethod(sb, context.ClassName, case_);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a single builder method
    /// </summary>
    private static void GenerateBuilderMethod(StringBuilder sb, string className, SemanticCaseInfo case_)
    {
        var methodName = case_.MethodName;
        var hasParameters = case_.Parameters.Length > 0;

        sb.Append($"        public static IS<{className}, T> {methodName}<T>(");

        if (hasParameters)
        {
            var paramList = string.Join(", ", case_.Parameters.Select(p =>
            {
                var paramType = ConvertSemanticTypeToConstructorType(p.Type);
                return $"{paramType} {p.Name}";
            }));
            sb.Append(paramList);
        }

        sb.AppendLine($") => new D.{methodName}<T>(");

        if (hasParameters)
        {
            var argList = string.Join(", ", case_.Parameters.Select(p => p.Name));
            sb.AppendLine($"            {argList}");
        }

        sb.AppendLine("        );");
        sb.AppendLine();
    }

    /// <summary>
    /// Converts semantic method parameter types to constructor parameter types
    /// </summary>
    private static string ConvertSemanticTypeToConstructorType(string semanticType)
    {
        // Convert "TS" to "T", and other generic parameter types
        return semanticType switch
        {
            "TS" => "T",
            "TR" => "T", // Generally, in constructors we store T
            _ when semanticType.StartsWith("TS") => semanticType.Replace("TS", "T"),
            _ => semanticType
        };
    }

    /// <summary>
    /// Capitalizes the first letter of a string
    /// </summary>
    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }
}
