using SemanticAlgebra.Control;
using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra.Free;

public abstract partial class Free<F> : IMonad<Free<F>>
    where F : IFunctor<F>
{
    //public static ISemantic1<Free<F>, TS, TR> Compose<TS, TI, TR>(ISemantic1<Free<F>, TS, TI> s, Func<TI, TR> f)
    //    => IAlias<TS>.Compose(s, f);

    //public static ISemantic1<Free<F>, T, IS<Free<F>, T>> Id<T>()
    //    => IAlias<T>.Id();

    //public static ISemantic1<Free<F>, IS<Free<F>, T>, IS<Free<F>, T>> JoinS<T>()
    //{
    //    throw new NotImplementedException();
    //}

    //public static ISemantic1<Free<F>, TS, IS<Free<F>, TR>> MapS<TS, TR>(Func<TS, TR> f)
    //{
    //    throw new NotImplementedException();
    //}

    //public static IS<Free<F>, T> Pure<T>(T x)
    //    => B.From(FreeF2<F>.B.Pure<T, Fix<K2<FreeF2<F>, T>>>(x).Fix());

    //public interface IAlias<TS> : Alias1.ISpec<Free<F>, TS, Fix<K2<FreeF2<F>, TS>>>
    //{
    //}

    //public static class B
    //{
    //    public static IS<Free<F>, T> From<T>(Fix<K2<FreeF2<F>, T>> value)
    //        => IAlias<T>.From(value);
    //}

    [Semantic1]
    public interface ISemantic<TS, out TR>
        : ISemantic1<Free<F>, TS, TR>
    {
        TR Pure(TS v);
        TR Roll(IS<F, IS<Free<F>, TS>> v);
    }

    public static ISemantic1<Free<F>, IS<Free<F>, T>, IS<Free<F>, T>> JoinS<T>()
        => new JoinSemantic<T>();

    public static ISemantic1<Free<F>, TS, IS<Free<F>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public static IS<Free<F>, T> Pure<T>(T x)
        => B.Pure(x);

    sealed class MapSemantic<TS, TR>(Func<TS, TR> Func) : ISemantic<TS, IS<Free<F>, TR>>
    {
        public IS<Free<F>, TR> Pure(TS v)
            => B.Pure(Func(v));

        public IS<Free<F>, TR> Roll(IS<F, IS<Free<F>, TS>> v)
            => B.Roll(v.Select(vf => vf.Select(Func)));
    }


    sealed class JoinSemantic<T> : ISemantic<IS<Free<F>, T>, IS<Free<F>, T>>
    {
        public IS<Free<F>, T> Pure(IS<Free<F>, T> v)
            => v;

        public IS<Free<F>, T> Roll(IS<F, IS<Free<F>, IS<Free<F>, T>>> v)
            => B.Roll(v.Select(vi => vi.Evaluate(this)));
    }

    internal sealed class InterpSemantic<M, T>(INaturalTransform<F, M> S)
            : ISemantic<T, IS<M, T>>
            where M : IMonad<M>
    {
        public IS<M, T> Pure(T v)
            => M.Pure(v);

        public IS<M, T> Roll(IS<F, IS<Free<F>, T>> v)
        {
            var x = v.Select(e => e.Interprete(S));
            var fx = S.Invoke(x);
            return fx.Join();
        }
    }


    //sealed class DistributeTransformImpl<G>
    //    : IDistributeTransform<Free<F>, G>
    //    where G : IFunctor<G>
    //{

    //    sealed class DistributeSemantic<T>
    //        : ISemantic<IS<G, T>, IS<G, IS<Free<F>, T>>>
    //    {
    //        public IS<G, IS<Free<F>, T>> Pure(IS<G, T> v)
    //            => v.Select(B.Pure);

    //        public IS<G, IS<Free<F>, T>> Roll(IS<F, IS<Free<F>, IS<G, T>>> v)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //    public IS<G, IS<Free<F>, T>> Distribute<T>(IS<Free<F>, IS<G, T>> fg)
    //        => fg.Evaluate(new DistributeSemantic<T>());
    //}

    //public static IDistributeTransform<Free<F>, G> DistributeTransform<G>()
    //    where G : IFunctor<G>
    //    => new DistributeTransformImpl<G>();
}

public static partial class FreePreludeExtension
{
    public static IS<Free<F>, T> LiftF<F, T>(this IS<F, T> e)
        where F : IFunctor<F>
        => Free<F>.B.Roll(e.Select(Free<F>.B.Pure));

    public static ISemantic1<Free<F>, T, T> LiftF<F, T>(this ISemantic1<F, T, T> s)
        where F : IFunctor<F>
        => new FreeLiftSemantic<F, T>(s);

    public static IS<M, T> Interprete<F, M, T>(this IS<Free<F>, T> e, INaturalTransform<F, M> semantic)
        where F : IFunctor<F>
        where M : IMonad<M>
        => e.Evaluate(new Free<F>.InterpSemantic<M, T>(semantic));

    sealed class FreeLiftSemantic<F, T>(
          ISemantic1<F, T, T> nest
      ) : Free<F>.ISemantic<T, T>
        where F : IFunctor<F>
    {
        public T Pure(T v) => v;

        public T Roll(IS<F, IS<Free<F>, T>> v)
            => v.Select(e => e.Evaluate(this)).Evaluate(nest);
    }

    //public static Free<F>.IAlias<TS>.ISemantic<TR> Prj<F, S, TS, TR>(this
    //    ISemantic1<Free<F>, TS, TR> s)
    //    where F : IFunctor<F>
    //    => (Free<F>.IAlias<TS>.ISemantic<TR>)s;
}

// let freef f a x = pure a | roll f x
// free f a ~ fix (freef f a)
// free f a = pure a | roll (f (free f a))
//       t  ~ a + f t 
//          = a + f (a + f t)
//          = a + f (a + f (a + f (a + ...
// fix (freef f a) = (freef f a) (fix (freef f a))
//      fix = a + f fix
//          = a + f ( a + f fix )
//          = a + f ( a + f ( a + f fix ) )

public sealed partial class FreeF2<F>
    : IFunctor2<FreeF2<F>>
    where F : IFunctor<F>
{
    public static class B
    {
        public static IS2<FreeF2<F>, TA, TB> Pure<TA, TB>(TA Value)
            => new D.Pure<TA, TB>(Value);
        public static IS2<FreeF2<F>, TA, TB> Roll<TA, TB>(IS<F, IS2<FreeF2<F>, TA, TB>> Value)
            => new D.Roll<TA, TB>(Value);
    }

    public static class D
    {
        public sealed record class Pure<TA, TB>(TA Value)
            : IS2<FreeF2<F>, TA, TB>
        {
            public TR Evaluate<TR>(ISemantic2<FreeF2<F>, TA, TB, TR> semantic)
                => semantic.Prj().Pure(Value);

            public IS2<FreeF2<F>, TRA, TRB> Select<TRA, TRB>(Func<TA, TRA> f, Func<TB, TRB> g)
                => new Pure<TRA, TRB>(f(Value));
        }

        public sealed record class Roll<TA, TB>(IS<F, IS2<FreeF2<F>, TA, TB>> Value)
            : IS2<FreeF2<F>, TA, TB>
        {
            public TR Evaluate<TR>(ISemantic2<FreeF2<F>, TA, TB, TR> semantic)
                => semantic.Prj().Roll(Value);

            public IS2<FreeF2<F>, TRA, TRB> Select<TRA, TRB>(Func<TA, TRA> f, Func<TB, TRB> g)
                => new Roll<TRA, TRB>(Value.Select(v => v.Select(f, g)));
        }
    }

    public interface ISemantic<TA, TB, TR>
        : ISemantic2<FreeF2<F>, TA, TB, TR>
        , ISemantic1<K21<FreeF2<F>, TA>, TB, TR>
    {
        TR Pure(TA value);
        TR Roll(IS<F, IS2<FreeF2<F>, TA, TB>> value);
    }

    sealed class FuncSemantic<TA, TB, TR>(Func<IS2<FreeF2<F>, TA, TB>, TR> Func) : ISemantic<TA, TB, TR>
    {
        public TR Pure(TA value)
            => Func(B.Pure<TA, TB>(value));

        public TR Roll(IS<F, IS2<FreeF2<F>, TA, TB>> value)
            => Func(B.Roll(value));
    }

    public static ISemantic2<FreeF2<F>, TA, TB, TR> Semantic<TA, TB, TR>(Func<IS2<FreeF2<F>, TA, TB>, TR> f)
        => new FuncSemantic<TA, TB, TR>(f);
}

public static partial class FreeF2Extension
{
    public static FreeF2<F>.ISemantic<TA, TB, TR> Prj<F, TA, TB, TR>(
        this ISemantic2<FreeF2<F>, TA, TB, TR> semantic
    )
        where F : IFunctor<F>
        => (FreeF2<F>.ISemantic<TA, TB, TR>)semantic;
}


public sealed partial class FreeF<F, A>
    : IFunctor<FreeF<F, A>>
    where F : IFunctor<F>
{
    [Semantic1]
    public interface ISemantic<TS, out TR>
        : ISemantic1<FreeF<F, A>, TS, TR>
    {
        TR Pure(A value);
        TR Roll(IS<F, TS> nest);
    }

    public static ISemantic1<FreeF<F, A>, TS, IS<FreeF<F, A>, TR>> MapS<TS, TR>(Func<TS, TR> f)
        => new MapSemantic<TS, TR>(f);

    public sealed class MapSemantic<TS, TR>(
        Func<TS, TR> Func
    ) : ISemantic<TS, IS<FreeF<F, A>, TR>>
    {
        public IS<FreeF<F, A>, TR> Roll(IS<F, TS> nest)
            => B.Roll(nest.Select(Func));

        public IS<FreeF<F, A>, TR> Pure(A value)
            => B.Pure<TR>(value);
    }

    public sealed class SemanticF<T>(
        ISemantic1<F, T, T> semantic,
        Func<A, T> pure
    ) : ISemantic<T, T>
    {
        public T Roll(IS<F, T> nest)
            => nest.Evaluate(semantic);

        public T Pure(A value)
            => pure(value);
    }

    sealed class MapNaturalTransform<TB>(Func<A, TB> func) : INaturalTransform<FreeF<F, A>, FreeF<F, TB>>
    {
        sealed class TransformSemantic<T>(Func<A, TB> func) : ISemantic<T, IS<FreeF<F, TB>, T>>
        {
            public IS<FreeF<F, TB>, T> Pure(A value)
                => FreeF<F, TB>.B.Pure<T>(func(value));

            public IS<FreeF<F, TB>, T> Roll(IS<F, T> nest)
                => FreeF<F, TB>.B.Roll(nest);
        }
        public IS<FreeF<F, TB>, T> Invoke<T>(IS<FreeF<F, A>, T> e)
            => e.Evaluate(new TransformSemantic<T>(func));
    }


    public static INaturalTransform<FreeF<F, A>, FreeF<F, TB>> NaturalTransform<TB>(Func<A, TB> f)
        => new MapNaturalTransform<TB>(f);
}
