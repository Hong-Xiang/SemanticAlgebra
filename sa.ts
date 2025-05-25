interface Kind1<F extends Kind1<F>> {
  readonly R?: unknown;
  readonly A?: unknown;
}
interface Kind2<F extends Kind2<F>> {
  readonly R?: unknown;
  readonly A?: unknown;
  readonly B?: unknown;
}
interface Exist1<FS extends Kind2<FS>> extends Kind1<Exist1<FS>> {
  readonly R: <R>(s: K2<FS, this["A"], R>) => R;
}

type K1<F extends Kind1<F>, A> = (F & { readonly A: A })["R"];
type K2<F extends Kind2<F>, A, B> = (F & { readonly A: A; readonly B: B })["R"];
type KS1<
  F extends Kind1<F> & { readonly Semantic: Kind2<F["Semantic"]> },
  A
> = (F & {
  readonly A: A;
})["R"];

interface OptionSemantic<in TI, out TO> {
  readonly Some: (x: TI) => TO;
  readonly None: () => TO;
}
interface OptionSemanticK2 extends Kind2<OptionSemanticK2> {
  readonly R: OptionSemantic<this["A"], this["B"]>;
}

type Option = Exist1<OptionSemanticK2>;

const OptionF: Pure<Option> = {
  pure:
    <A>(a: A) =>
    <R>(s: OptionSemantic<A, R>) =>
      s.Some(a),
};

interface Pure<F extends Kind1<F>> {
  readonly pure: <A>(a: A) => K1<F, A>;
}

interface ValSemantic<in TI, out TO> {
  readonly val: (x: TI) => TO;
}
interface ValSK2 extends Kind2<ValSK2> {
  readonly R: ValSemantic<this["A"], this["B"]>;
}
type Val = Exist1<ValSK2>;
interface AddS<in TI, out TO> {
  readonly add: (l: TI, r: TI) => TO;
}
interface AddSK2 extends Kind2<AddSK2> {
  readonly R: AddS<this["A"], this["B"]>;
}
type Add = Exist1<AddSK2>;

interface CompS<in TI, out TO> extends ValSemantic<TI, TO>, AddS<TI, TO> {}
interface CompSK2 extends Kind2<CompSK2> {
  readonly R: CompS<this["A"], this["B"]>;
}
type Comp = Exist1<CompSK2>;

function useVal<T>(x: K1<Val, T>): void {
  return useComp(x);
}
function useComp<T>(x: K1<Comp, T>): void {
  return useVal(x);
}
