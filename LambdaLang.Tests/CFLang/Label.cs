namespace LambdaLang.Tests.CFLang;

public sealed class Label(string? name)
{
    public string? Name { get; } = name;

    public static T Create<T>(string name, Func<Label, T> f)
        => f(new Label(name));
}
