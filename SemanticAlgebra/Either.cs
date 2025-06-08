using SemanticAlgebra.Data;
using SemanticAlgebra.Syntax;

namespace SemanticAlgebra;

public abstract partial class Either<TL> : IFunctor<Either<TL>>
{
    [Semantic1]
    public interface ISemantic<in TS, out TR>
        : ISemantic1<Either<TL>, TS, TR>
    {
        TR Left(TL value);
        TR Right(TS value);
    }
}