using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;
using System.Collections.Immutable;

namespace LambdaLang.Tests.CFLang;

public sealed record class ShowState(
    int ValueIndex,
    ImmutableDictionary<Value, int> ValueIds,
    int LabelIndex,
    ImmutableDictionary<Label, int> LabelIds,
    int Indentation
)
{
    public static ShowState Empty => new ShowState(
        0, ImmutableDictionary<Value, int>.Empty,
        0, ImmutableDictionary<Label, int>.Empty,
        0);

    public ShowState ValueId(Value v, out int id)
    {
        if (ValueIds.TryGetValue(v, out id))
        {
            return this;
        }
        else
        {
            id = ValueIndex;
            return this with
            {
                ValueIndex = ValueIndex + 1,
                ValueIds = ValueIds.Add(v, id)
            };
        }
    }
    public ShowState LableId(Label v, out int id)
    {
        if (LabelIds.TryGetValue(v, out id))
        {
            return this;
        }
        else
        {
            id = LabelIndex;
            return this with
            {
                LabelIndex = LabelIndex + 1,
                LabelIds = LabelIds.Add(v, id)
            };
        }
    }
    public ShowState Indent() => this with { Indentation = Indentation + 1 };
    public ShowState Unindent() => this with { Indentation = Indentation - 1 };
    public string Line(string s) => $"{new string('\t', Indentation)}{s}{Environment.NewLine}";
}

public abstract partial class ShowF : IFunctor<ShowF>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<ShowF, TS, TR>
    {
        TR GetValueId(Value value, Func<int, TS> next);
        TR GetLabelId(Label value, Func<int, TS> next);
        TR Indent(TS next);
        TR Unindent(TS next);
        TR Write(string text, TS next);
        TR WriteLine(string text, TS next);
    }


    public static ISemantic1<ShowF, TS, IS<ShowF, TR>> MapS<TS, TR>(Func<TS, TR> f)
            => new MapSemantic<TS, TR>(f);

    sealed class MapSemantic<TS, TR>(Func<TS, TR> func)
        : ISemantic<TS, IS<ShowF, TR>>
    {
        public IS<ShowF, TR> GetLabelId(Label value, Func<int, TS> lookup)
            => B.GetLabelId(value, id => func(lookup(id)));

        public IS<ShowF, TR> GetValueId(Value value, Func<int, TS> lookup)
            => B.GetValueId(value, id => func(lookup(id)));


        public IS<ShowF, TR> Indent(TS a)
            => B.Indent(func(a));

        public IS<ShowF, TR> Unindent(TS a)
            => B.Unindent(func(a));

        public IS<ShowF, TR> Write(string text, TS a)
            => B.Write(text, func(a));

        public IS<ShowF, TR> WriteLine(string text, TS a)
            => B.WriteLine(text, func(a));
    }

}
