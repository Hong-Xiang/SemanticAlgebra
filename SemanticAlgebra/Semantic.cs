namespace SemanticAlgebra;

// Semantic1 encodes f s -> r
// TF<TS> is not valid dotnet type due to lack of builtin higher kinded type support
// Unfortunately, IAlgebra1<TF, TS, TR> ~ f s -> r is not enforced by dotnet type system,
// Thus we need to check it manually
// Semantic could be viewd as a generalization of object algebra,
// which could be considered as a generalization of visitor pattern.
public interface ISemantic1<out TF, in TS, out TR>
    where TF : IKind1<TF>
{
}

// forall r. semantic<f, s, r> -> r ~ f s
// f (g a)
// ~ forall r1. sem<f, g a, r1>
// ~ forall r1. sem<f, forall r2. sem<g, a, r2>, r1>
