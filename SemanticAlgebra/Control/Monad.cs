namespace SemanticAlgebra.Control;

public interface IMonad<TF> : IApplicative<TF>, IBind<TF> 
    where TF : IMonad<TF>
{
}
