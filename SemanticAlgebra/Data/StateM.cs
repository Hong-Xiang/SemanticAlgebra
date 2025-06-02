using SemanticAlgebra.Control;

namespace SemanticAlgebra.Data;

// state f a b
//    get :: f a b
//    put :: a -> f a b

// valid impl :
//  f a b = f s s = m s

//    run :: (s, f a) -> a
public class StateM<M> : IKind11<StateM<M>>
    where M : IMonad<M>
{
    public interface ISemantic<out TA, in TB, out TR>
        : ISemantic11<StateM<M>, TA, TB, TR>
    {
        TR Get();
        TR Put(TB a);
    }


    public static ISemantic11<StateM<M>, TA, TB, TR> Semantic<TA, TB, TR>(Func<IS11<StateM<M>, TA, TB>, TR> f)
    {
        throw new NotImplementedException();
    }
}