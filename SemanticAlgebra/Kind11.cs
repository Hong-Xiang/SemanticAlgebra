namespace SemanticAlgebra;

// encoding f a b, where f is profunctor
public interface IKind11<TF>
    where TF : IKind11<TF>
{
    // semantic is encoding of f a b -> r
    static abstract ISemantic11<TF, TA, TB, TR> Semantic<TA, TB, TR>(Func<IS11<TF, TA, TB>, TR> f);
}

// f a b
public interface ISemantic11<out F, out TA, in TB, out TR>
    where F : IKind11<F>
{
}

public interface IS11<in TF, in TA, out TB>
    where TF : IKind11<TF>
{
    TR Evaluate<TR>(ISemantic11<TF, TA, TB, TR> semantic);
}