using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra.Control;

public interface IMonad<TF> : IApplicative<TF>, IBind<TF> 
    where TF : IMonad<TF>
{
}
