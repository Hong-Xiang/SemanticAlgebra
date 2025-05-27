# SemanticAlgebra
Library for generalized object algebra and lightweight higher-kinded type for dotnet

## Encoding Higher Kinded Types - Generics Over Generics

### Challenge

While we can use encoding above for encoding higher kinded types of reference types,
i.e. when `IS<F, T>` is actual reference type/boxed value type,
we can't use it for stack only value types `ref struct` in,
e.g.
for any functor `F`, we need its `id` semantic `ISemantic<F, T, IS<F, T>>`,
while the argument `IS<F, T>` works fine since it is actually not directly used by semantic encoding,
which is essentially a encoding like `<TFT>Foo(TFT t) where TFT : IS<F, T>`, thus allowing to use `ref struct T : IS<F, T>` as a argument,
for those functions return `IS<F, T>` we don't have a way to hide the actual type `IS<F, T>` from the signature,
we can't have a function like `T Foo<T>() where T : IS<F, T>` since it is actually a existential type,
thus we need encoding `exist T. T Foo() where T : IS<F, T>` which is not supported by C#,
```
interface ExistFT<F, T, TFT, TA, TB> 
    where TFT : IS<F, T>
{
}
interface FuncReturnFT<F, T, TA, TB> {
    TFT Foo<TFT>(ExistFT<F, T, TFT, TA, TB> exist) where TFT : IS<F, T>;
}
```


for func `f s -> r` we can encode it as
```
interface ISemantic<F, S, R> {} // semi-type safe
```
for `f s` we encode it as
```
interface IS<F, T> {
    R Evaluate<R>(ISemantic<F, T, R> s);
}
```
for func `s -> f r` we can encode it as
```
interface ICoSemantic<F, S, R> {
    O CoEvaluate<O>(S s, ISemantic<F, R, O> fro);
}
```
Encoding above works for `ref struct` e.g.
```
sealed class Option {} // brand

interface IOptionSemantic<TS, TR> : ISemantic<Option, TS, TR> {
    TR None();
    TR Some(TS s);
}
ref struct Some<T>(T value) : IS<Option, T> {
    public R Evaluate<R>(ISemantic<Option, T, R> s) => ((IOptionSemantic<T, TR>)s).Some(value); // semi-type safe
}
ref struct None<T> : IS<Option, T> {
    public R Evaluate<R>(ISemantic<Option, T, R> s) => ((IOptionSemantic<T, TR>)s).None(); // semi-type safe
}


// pure :: t -> option t
// pure x = Some x
sealed class Pure<T> : ICoSemantic<Option, T, T> {
    public R CoEvaluate<R>(T s, ISemantic<Option, T, R> c) => ((IOptionSemantic<T, R>)c).Some(s); // semi-type safe
}
```

But a func `f s -> f r` would be either
```
interface ICoSemantic<F, S, R> {
    O CoEvaluate<O>(IS<F, S> s, ISemantic<F, R, O> s);
}
```
or
```
ISemantic<F, S, IS<F, R>>
```
with `IS<F, T>` in their signature, which is not what we want

