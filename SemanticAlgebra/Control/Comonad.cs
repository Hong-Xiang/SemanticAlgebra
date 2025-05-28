namespace SemanticAlgebra.Control;

public interface IComonad<TF> : IExtend<TF>, IExtract<TF>
    where TF : IComonad<TF>
{
}
