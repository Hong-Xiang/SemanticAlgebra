using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SemanticAlgebra.SourceGenerator;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SourceGeneratorCoreTest;

public sealed class EnhancedSemanticKindSourceGeneratorTest(ITestOutputHelper Output)
{
    private readonly ITestOutputHelper _output = Output;

    [Fact]
    public void OptionClassShouldGenerateAllComponents()
    {
        var optionCode = @"
using SemanticAlgebra.Control;
using SemanticAlgebra.Syntax;
using SemanticAlgebra;

namespace SemanticAlgebra.Option;

[SemanticKind1Brand]
public sealed partial class Option : IMonad<Option>
{
    public interface ISemantic<in TS, out TR> : ISemantic1<Option, TS, TR>
    {
        TR None();
        TR Some(TS value);
    }
}";

        var result = RunSourceGenerator(optionCode);

        // Should generate 3 files: Prj extension, Data definitions, and Builders
        Assert.Equal(3, result.GeneratedSources().ToArray().Length);

        // Check that all expected files are generated
        var sourceFileNames = result.GeneratedSources().Select(s => s.HintName).ToArray();
        Assert.Contains("OptionExtension.Prj.g.cs", sourceFileNames);
        Assert.Contains("Option.DataDefinitions.g.cs", sourceFileNames);
        Assert.Contains("Option.Builders.g.cs", sourceFileNames);

        // Verify Prj extension content
        var prjExtension = result.GeneratedSources().First(s => s.HintName == "OptionExtension.Prj.g.cs");
        Assert.Contains("public static Option.ISemantic<TS, TR> Prj<TS, TR>", prjExtension.SourceText.ToString());

        // Verify Data definitions content
        var dataDefinitions = result.GeneratedSources().First(s => s.HintName == "Option.DataDefinitions.g.cs");
        var dataContent = dataDefinitions.SourceText.ToString();
        Assert.Contains("public static partial class D", dataContent);
        Assert.Contains("public sealed record class None<T>", dataContent);
        Assert.Contains("public sealed record class Some<T>", dataContent);
        Assert.Contains("semantic.Prj().None()", dataContent);
        Assert.Contains("semantic.Prj().Some(Value)", dataContent);

        // Verify Builders content
        var builders = result.GeneratedSources().First(s => s.HintName == "Option.Builders.g.cs");
        var buildersContent = builders.SourceText.ToString();
        Assert.Contains("public static partial class B", buildersContent);
        Assert.Contains("public static IS<Option, T> None<T>()", buildersContent);
        Assert.Contains("public static IS<Option, T> Some<T>(T value)", buildersContent);
        Assert.Contains("new D.None<T>", buildersContent);
        Assert.Contains("new D.Some<T>(", buildersContent);

        _output.WriteLine("Generated Prj Extension:");
        _output.WriteLine(prjExtension.SourceText.ToString());
        _output.WriteLine("\nGenerated Data Definitions:");
        _output.WriteLine(dataContent);
        _output.WriteLine("\nGenerated Builders:");
        _output.WriteLine(buildersContent);
    }

    [Fact]
    public void ArithClassShouldGenerateAllComponents()
    {
        var arithCode = @"
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;
using SemanticAlgebra;

namespace LambdaLang.Language;

[SemanticKind1Brand]
public sealed partial class Arith : IFunctor<Arith>
{
    public interface ISemantic<in TS, out TR> : ISemantic1<Arith, TS, TR>
    {
        TR Add(TS l, TS r);
        TR Mul(TS l, TS r);
    }
}";

        var result = RunSourceGenerator(arithCode);

        // Should generate 3 files
        Assert.Equal(3, result.GeneratedSources().ToArray().Length);

        // Verify Data definitions content for Arith
        var dataDefinitions = result.GeneratedSources().First(s => s.HintName == "Arith.DataDefinitions.g.cs");
        var dataContent = dataDefinitions.SourceText.ToString();
        Assert.Contains("public sealed record class Add<T>(", dataContent);
        Assert.Contains("public sealed record class Mul<T>(", dataContent);
        Assert.Contains("T L,", dataContent);
        Assert.Contains("T R", dataContent);
        Assert.Contains("semantic.Prj().Add(L, R)", dataContent);
        Assert.Contains("semantic.Prj().Mul(L, R)", dataContent);

        // Verify Builders content for Arith
        var builders = result.GeneratedSources().First(s => s.HintName == "Arith.Builders.g.cs");
        var buildersContent = builders.SourceText.ToString();
        Assert.Contains("public static IS<Arith, T> Add<T>(T l, T r)", buildersContent);
        Assert.Contains("public static IS<Arith, T> Mul<T>(T l, T r)", buildersContent);
        Assert.Contains("new D.Add<T>(", buildersContent);
        Assert.Contains("new D.Mul<T>(", buildersContent);

        _output.WriteLine("Generated Arith Data Definitions:");
        _output.WriteLine(dataContent);
        _output.WriteLine("\nGenerated Arith Builders:");
        _output.WriteLine(buildersContent);
    }

    [Fact]
    public void AppClassShouldGenerateAllComponents()
    {
        var appCode = @"
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Language;

[SemanticKind1Brand]
public sealed partial class App : IFunctor<App>
{
    public interface ISemantic<in TS, out TR> : ISemantic1<App, TS, TR>
    {
        TR Apply(TS f, TS x);
    }
}";

        var result = RunSourceGenerator(appCode);

        // Should generate 3 files
        Assert.Equal(3, result.GeneratedSources().ToArray().Length);

        // Verify Data definitions content for App
        var dataDefinitions = result.GeneratedSources().First(s => s.HintName == "App.DataDefinitions.g.cs");
        var dataContent = dataDefinitions.SourceText.ToString();
        Assert.Contains("public sealed record class Apply<T>(", dataContent);
        Assert.Contains("T F,", dataContent);
        Assert.Contains("T X", dataContent);
        Assert.Contains("semantic.Prj().Apply(F, X)", dataContent);

        // Verify Builders content for App
        var builders = result.GeneratedSources().First(s => s.HintName == "App.Builders.g.cs");
        var buildersContent = builders.SourceText.ToString();
        Assert.Contains("public static IS<App, T> Apply<T>(T f, T x)", buildersContent);
        Assert.Contains("new D.Apply<T>(", buildersContent);

        _output.WriteLine("Generated App Data Definitions:");
        _output.WriteLine(dataContent);
        _output.WriteLine("\nGenerated App Builders:");
        _output.WriteLine(buildersContent);
    }

    [Fact]
    public void ClassWithoutISemanticShouldNotGenerate()
    {
        var codeWithoutISemantic = @"
using SemanticAlgebra.Syntax;

namespace Test;

[SemanticKind1Brand]
public sealed partial class TestClass
{
    // No ISemantic interface
}";

        var result = RunSourceGenerator(codeWithoutISemantic);

        // Should generate no files
        Assert.Empty(result.GeneratedSources());
    }

    [Fact]
    public void ClassWithoutSemanticKind1BrandShouldNotGenerate()
    {
        var codeWithoutAttribute = @"
using SemanticAlgebra;

namespace Test;

public sealed partial class TestClass
{
    public interface ISemantic<in TS, out TR> : ISemantic1<TestClass, TS, TR>
    {
        TR Test();
    }
}";

        var result = RunSourceGenerator(codeWithoutAttribute);

        // Should generate no files
        Assert.Empty(result.GeneratedSources());
    }

    private static GeneratorDriverRunResult RunSourceGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree, Utility.Kind1BrandAttribute],
            references: ReferenceAssemblies.Net80,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new SemanticKindSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        return driver.RunGenerators(compilation).GetRunResult();
    }
}
