namespace LambdaLang.Tests.CFLang;

public sealed class Value(string? name)
{
    public string? Name { get; } = name;

    public static T Create<T>(string name, Func<Value, T> f)
        => f(new Value(name));
}
