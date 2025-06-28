using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
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

    public (ShowState State, int Index) ValueId(Value v)
    {
        if (ValueIds.TryGetValue(v, out var id))
        {
            return (this, id);
        }
        else
        {
            id = ValueIndex;
            return (this with
            {
                ValueIndex = ValueIndex + 1,
                ValueIds = ValueIds.Add(v, id)
            }, id);
        }
    }
    public (ShowState State, int Index) LableId(Label v)
    {
        if (LabelIds.TryGetValue(v, out var id))
        {
            return (this, id);
        }
        else
        {
            id = LabelIndex;
            return (this with
            {
                LabelIndex = LabelIndex + 1,
                LabelIds = LabelIds.Add(v, id)
            }, id);
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
        TR GetIndentation(Func<int, TS> next);
    }


    public static ISemantic1<ShowF, TS, IS<ShowF, TR>> MapS<TS, TR>(Func<TS, TR> f)
            => new MapSemantic<TS, TR>(f);

    sealed class MapSemantic<TS, TR>(Func<TS, TR> func)
        : ISemantic<TS, IS<ShowF, TR>>
    {
        public IS<ShowF, TR> GetIndentation(Func<int, TS> next)
            => B.GetIndentation(level => func(next(level)));

        public IS<ShowF, TR> GetLabelId(Label value, Func<int, TS> lookup)
            => B.GetLabelId(value, id => func(lookup(id)));

        public IS<ShowF, TR> GetValueId(Value value, Func<int, TS> lookup)
            => B.GetValueId(value, id => func(lookup(id)));


        public IS<ShowF, TR> Indent(TS a)
            => B.Indent(func(a));

        public IS<ShowF, TR> Unindent(TS a)
            => B.Unindent(func(a));
    }
}

public static partial class ShowFExtension
{
    sealed class ShowFPrintFreeSemantic
        : ShowF.ISemantic<Func<int, string>, Func<int, string>>
    {
        private Dictionary<Label, int> Labels = [];
        private Dictionary<Value, int> Values = [];

        public Func<int, string> GetIndentation(Func<int, Func<int, string>> next)
            => l => next(l)(l);

        public Func<int, string> GetLabelId(Label value, Func<int, Func<int, string>> next)
        {
            if (Labels.TryGetValue(value, out var result))
            {
                return next(result);
            }
            else
            {
                var id = Labels.Count;
                Labels.Add(value, id);
                return next(id);
            }
        }

        public Func<int, string> GetValueId(Value value, Func<int, Func<int, string>> next)
        {
            if (Values.TryGetValue(value, out var result))
            {
                return next(result);
            }
            else
            {
                var id = Labels.Count;
                Values.Add(value, id);
                return next(id);
            }
        }

        public Func<int, string> Indent(Func<int, string> next)
            => l => next(l + 1);

        public Func<int, string> Unindent(Func<int, string> next)
            => l => next(l - 1);

    }

    public static string Print(this IS<Free<ShowF>, string> e)
        => e.Select<Free<ShowF>, string, Func<int, string>>(x => l => x).Evaluate(new ShowFPrintFreeSemantic().LiftF())(0);
}
