using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAlgebra;

// for any type f a, it is isomorphism to forall r. (f a -> r) -> r
// for our higher kinded type encoding, 
// ISemantic<f, a, r> ~ (f a) -> r
// thus IS<f, T> is just wrapper for forall r. (f a -> r) -> r,
// thus IS<f, T> ~  f t, with out explict 
public interface IS<in TF, out T> where TF : IKind1<TF>
{
    TR Evaluate<TR>(ISemantic1<TF, T, TR> semantic);
}
