using SemanticAlgebra;
using SemanticAlgebra.Data;

namespace LambdaLang.Tests.CFLang;

public sealed record class ShowState(
    int ValueIndex,
    int LabelIndex,
    int Indentation
)
{
    public static ShowState Empty => new ShowState(0, 0, 0);

    public ShowState NextValue(out int index)
    {
        index = ValueIndex;
        return this with { ValueIndex = ValueIndex + 1 };
    }

    public ShowState NextLabel(out int index)
    {
        index = LabelIndex;
        return this with { LabelIndex = LabelIndex + 1 };
    }

    public ShowState Indent() => this with { Indentation = Indentation + 1 };
    public ShowState Unindent() => this with { Indentation = Indentation - 1 };
    public string Line(string s) => $"{new string('\t', Indentation)}{s}{Environment.NewLine}";
}

public interface ShowAlg : IAlgebraM<ShowState, string>
{
}


