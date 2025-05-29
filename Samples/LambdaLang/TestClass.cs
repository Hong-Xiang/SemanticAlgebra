using SemanticAlgebra.Syntax;

namespace LambdaLang;

/// <summary>
/// Test class to demonstrate the SemanticAlgebra Source Generator
/// </summary>
[SemanticKind1BrandAttribute]
public partial class TestClass
{
    public string ExistingProperty { get; set; } = "Original";
    
    public void TestMethod()
    {
        Console.WriteLine($"Existing: {ExistingProperty}");
        Console.WriteLine($"Generated: {SourceGenTest}");
        Console.WriteLine($"Generated: {SourceGenTest2}");
    }
}
