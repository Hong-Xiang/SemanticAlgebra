using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface Bind
    : IFunctor<Bind>
    , IWithAlgebra<Bind, ShowAlgebra, string>
    , IEvalAlgebra<Bind>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<Bind, TS, TR>
    {
        TR Let(Identifier name, TS expr, TS body);
        TR LetRec(Identifier name, TS expr, TS body);
    }

    static ISemantic1<Bind, string, string> IWithAlgebra<Bind, ShowAlgebra, string>.Get()
        => new BindShowFolder();

    static ISemantic1<Bind, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<Bind>.Get<M>()
        => new BindEvalFolder<M>();
}

public sealed class BindShowFolder : Bind.ISemantic<string, string>
{
    public string LetRec(Identifier name, string expr, string body)
        => $"(letrec {name.Name} = {expr} in {body})";


    string Bind.ISemantic<string, string>.Let(Identifier name, string expr, string body)
        => $"(let {name.Name} = {expr} in {body})";
}

sealed class BindEvalFolder<M> : Bind.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> Let(Identifier name, IS<M, ISigValue> expr, IS<M, ISigValue> body)
        => from v in expr
           from r in M.Local(s => s.Add(name, v), body)
           select r;

    public IS<M, ISigValue> LetRec(Identifier name, IS<M, ISigValue> expr, IS<M, ISigValue> body)
        =>
            from env in M.Get()
            from exp in expr
            let expf = exp switch
            {
                SigClosure<M, ISigValue> c => GetRecClosure(name, c),
                _ => throw new EvalRuntimeException("let-rec only support lambda definition")
            }
            from r in M.Local(s => s.Add(name, expf), body)
            select r;

    public static SigClosure<M, ISigValue> GetRecClosure(
        Identifier name,
        SigClosure<M, ISigValue> original
    )
    {
        var result = new SigClosure<M, ISigValue>(original.Name, original.Env, original.Body);
        result.Env = result.Env.Add(name, result);
        return result;
    }
}