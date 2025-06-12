using System.Collections.Immutable;
using SemanticAlgebra;
using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Free;
using SemanticAlgebra.Syntax;

namespace LambdaLang.Tests.LambdaLang.Language;

public partial interface App
    : IFunctor<App>
    , IImplements<App, ShowAlgebra, string>
    , IEvalAlgebra<App>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR> : ISemantic1<App, TS, TR>
    {
        TR Apply(TS f_, TS x);
    }

    static ISemantic1<App, string, string> IImplements<App, ShowAlgebra, string>.Get()
        => new AppShowFolder();

    static ISemantic1<App, IS<M, ISigValue>, IS<M, ISigValue>> IEvalAlgebra<App>.Get<M>()
        => new AppEvalFolder<M>();

    static ISemantic1<App, IS<Free<EvalF>, ISigValue>, IS<Free<EvalF>, ISigValue>> IEvalAlgebra<App>.GetFree()
        => new AppFreeFolder();
}

public sealed class AppShowFolder : App.ISemantic<string, string>
{
    string App.ISemantic<string, string>.Apply(string f, string x)
        => $"{f}({x})";
}

public sealed class AppEvalFolder<M> : App.ISemantic<IS<M, ISigValue>, IS<M, ISigValue>>
    where M : IMonadState<M, ImmutableDictionary<Identifier, ISigValue>>
{
    public IS<M, ISigValue> Apply(IS<M, ISigValue> f, IS<M, ISigValue> x)
        => from f_ in f
           from x_ in x
           from r in f_ switch
           {
               SigClosure<M, ISigValue> c =>
                   M.Local(s => c.Env.Add(c.Name, x_), c.Body),
               _ => throw new EvalRuntimeException(
                   $"Function application requires a function and an value argument, got {f_} {x_}")
           }
           select r;
}

public sealed class AppFreeFolder
    : App.ISemantic<
    IS<Free<EvalF>, ISigValue>,
    IS<Free<EvalF>, ISigValue>>
{
    public IS<Free<EvalF>, ISigValue> Apply(IS<Free<EvalF>, ISigValue> f_, IS<Free<EvalF>, ISigValue> x)
        => from f in f_
           from x_ in x
           from r in f switch
           {
               SigClosureF<EvalF, ISigValue> c =>
                   EvalF.Local(s => c.Env.Add(c.Name, x_), c.Body),
               _ => throw new EvalRuntimeException(
                   $"Function application requires a function and an value argument, got {f_} {x_}")
           }
           select r;
}