using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Control;

public interface IComonad<TF> : IExtend<TF>, IExtract<TF>
    where TF : IComonad<TF>
{
}
