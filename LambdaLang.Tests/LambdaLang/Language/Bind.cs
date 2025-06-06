using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Bind : IFunctor<Bind>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Bind, TS, TR>
    {
        TR Let(Identifier name, TS expr, TS body);
    }
}

public sealed class BindShowFolder : Bind.ISemantic<string, string>
{
    string Bind.ISemantic<string, string>.Let(Identifier name, string expr, string body)
        => $"(let {name.Name} = {expr} in {body})";
}

sealed class BindEvalFolder<M> : Bind.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> Let(Identifier name, IS<M, ISigValue> expr, IS<M, ISigValue> body)
        => from v in expr
           from _ in M.Modify(s => s.Add(name, v))
           from r in body
           select r;
}