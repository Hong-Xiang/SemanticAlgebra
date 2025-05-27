using SemanticAlgebra.Data;

namespace SemanticAlgebra.Control;

public interface IApplicative<TF> : IApply<TF>, IPure<TF>
    where TF : IApplicative<TF>
{
}
