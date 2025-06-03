namespace SemanticAlgebra.Control;

public interface IMonad<TF> : IApplicative<TF>, IBind<TF>
    where TF : IMonad<TF>
{
    static ISemantic1<TF, Func<TS, TR>, ISemantic1<TF, TS, IS<TF, TR>>> IApply<TF>.ApplyS<TS, TR>()
        => TF.Id<Func<TS, TR>>().Compose(ff =>
            TF.Semantic<TS, IS<TF, TR>>(fs =>
                from f in ff
                from s in fs
                select f(s))
        );
}