using System.Collections.Immutable;

namespace ExprFTest;

// variables are holes in the expr
// thus e.g. add (hold v, lit 1) etc

// basically term v e  hole v | expr e 
// and final term is its fixed point (recursive over e)
// fix (term v) = (term v) (fix (term v)) = hole v | expr (fix (term v)) 
//              = hold v | expr (hole v | expr (fix (term v)))

// then let x = e in e'
// (C# syntax:
//  from x in e
//  select e')
// could be expressed as seq(let(v, e), e')
// where in e', the usage of e is replaced by v
// another representation is bind e (v -> e')
// which is requires function in AST
// it would make substitution much easier,
// but it might make the mattern patching on AST harder

// convert from bind to seq
// bind e f: (v -> e') = seq(let(v, e), f(v))
// from seq to bind
// seq(let(vname, e), e') = bind e v -> subst vname to v in e'

// so the grammar is
// atom x = lit n | add x x
// term = var v | atom term
// seqs = term | (let(v, atom term), seqs)

// alg :: f a -> a
// algM :: m a -> a, 

// cont = lit n | AddressOf symbol
// atomF v = cont | add v v
// exprF v e = val v | atom e

// expr = free atom v

// stmt r a v e = let v e | load v a | store a e | jump r e[] | ret | ret e
// seqs s n = single s | append (n, s)

// term l e = retU | retV e | jump l e[] 
// stmtF l a v e n = let v e n | load v a n | store a e n
// body = free (stmtF l a v expr) (term l)

// IIdentifier

// IValueIdentifier

// IIdentifier<TRegion, TAddress, 

// pair a b = (a, b), functor over b
// seqs s n = s | (n, s) = free (pair s) n

// expr = fix (expr v) = free atom v

// bb r a v = fix (seqs (stmt r a v expr)) 

// body r j b  = b | (j, b)

// join r j = blk r (body r j b) | rec r (body r j b)
// prog = fix (join r a v) = blk r (b | (prog, b) | rec r (b | (prog, b)))

// recursive
// atom = lit n | add (expr v) (expr v)
// expr v = val v | atom
// stmt r a v = let v (expr v) | load v a | store a (expr v) | jump r (expr v)[] | ret | ret (expr v)
// seqs r a v = stmt r a v | comp (stmt r a v) (seqs r a v) 
// region r a v = seqs r a v | block r (region r a v)  | loop r (region r a v)

// prog = fix (region r (fix (seqs (stmt r a v (free atom v))))

// now focus on small simple part
// atom e = lit n | add e e
// expr = free atom v = var v | atom expr

// term = end | ret expr
// stmt = let v (expr v)

// comp s p = (p, s)
// body s a = (fix (comp s), a)
// basic-block = body stmt term

// region-body = (region-defs, basic-block)
// region = blk(args, region-body)
//        | loop(label, args, region-body)
//        | if(val, region-body, region-body)   
//        | switch(val, (lit, region-body)[])
//        | region(label)

// region-def = letB(label, region : block) | letL region : loop
// region-defs = region-def[]

// region-defs and region-body can be modeled with
// seqs s n = s | (n, s)

// rbody x = free region-def x

// region-body = rbody basic-block
// region-defs = rbody region

// region-def = letB(label, blk) | letL(loop)
// blk = blk(args, region-body)
// loop = loop(label, args, region-body)

// region rbF rlF l b = rbF b | rlF b | if(val, b, b) | switch(val, b, (lit, b)[]) | region l
// blkF b = (args, b)
// loopF l b  = (l, args, b)
// rdef rb rl l n = letB l rb n | letL rl n
// rbody rbF rlF l x b = free (rdef (rb b) (rl b) l) x

// region blkF (rlF l) l (rbody (blkF (rbody   

// b = rbody blkF (rlF l) l basic-block b

// Grammar
// region l = blk | loop | if(val, region-body, region-body) | switch(val, region-body, (lit, region)[]) | region-ref l 
// block l = block(l, val[], region-body)
// loop l = loop(l, val[], region-body)
// region-body l = basic-block (region l) | letB (block l, region-body) | letL (loop l, region-body)

// Recursive

// rb l b = bb | letB(blockF b, b) | letL(looF b, b)
// rdf l b = (l, val[], b)
// rbf l b = letB (redf l b, b) | letL (redf l b, b)
// region-body l r = free (rbf l) (bb r)
// region l b r = block(redf l b)
//              | loop(redf l b)
//              | if(val, b, b)
//              | switch(val, b, (lit, b)[])
//              | region-ref l

// r = region l (region-body l r) r

// Recursive -- Refine

// atom e = lit n | add e e | ...
// expr = free atom v
// term e r = end | ret e | br r e[] | BrIf(e (r e[]) (r e[]) | Swich(e, (r e[]), (lit, r e[])[]) | ... 
// addr a = symbol | member path a | ...
// stmt a e n = let v e n | get v a n | set a e n | ... | push e | pop | ...
// comp stmtf x = free stmtf x

// bb sf e r = comp sf (term e r)

// def b = (l, v[], b)
// named-region b = block (def b) | loop (def b)
// rbf b = letB (def b, b) | letL (def b, b)
// rbd r = free named-region (bb [sf] [e] r)

// rbd x = free named-region x

// scf b = | named (named-region b)
//         | if(v, b, b)
//         | switch(v, b, (liti32, b)[])
//         | ...

//         | label l 

// region-body = rbd (bb s e r)

// rg r = scf (rbd (bb  ))
// region = free rg l 
//      = scf (rbd rg) | label l 

// grammar - refine
// atom e = lit n | add e e | ...
// addr a = symbol | member path a | ...
// expr = free atom v
// stmt e n = let v e n | get v (e: a) n | set (e: a) e n | ... | push e | pop | ...
// join r e = (r, e[])
// term e r = End | Ret e | Br(join r e) | BrIf(e, join r e, join r e) | Swich(e, join r e, (lit, join r e)[]) | ... 
// comp stmtf x = free stmtf x

// bb sf e r = comp sf (term e r)

// def b = (l, v[], b)
// named-region b = block (def b) | loop (def b)
// region-body b = let (named-region b, b)
// regionDefinition r = free region-body (bb [sf] [e] r)
// region = free RegionDefinition | label l

// rbd x = free named-region x

// scf b = | named (named-region b)
//         | if(v, b, b)
//         | switch(v, b, (liti32, b)[])
//         | ...

//         | label l 

// region-body = rbd (bb s e r)

// rg r = scf (rbd (bb  ))
// region = free rg l 
//      = scf (rbd rg) | label l 

interface IRegionSemantic<TLabel, in TB, out TO>
{
    TO Block(TLabel label, TB body);
    TO Loop(TLabel label, TB body);
    TO Label(TLabel label);
}

interface IRegionFoldSemantic<TLabel, TP, TBI, TRI, TBO, TRO>
    : ISeqPairSemantic<TRI, TP, TBI, TBO>
    , IRegionSemantic<TLabel, TBI, TRO>
{
}

interface IRegion<TLabel, out TB>
{
    TLabel Label { get; }
    TBR Eval<TBR>(IRegionSemantic<TLabel, TB, TBR> semantic);
    IRegion<TLabel, TRR> Select<TRR>(Func<TB, TRR> f);
}

sealed record class BlockR<TLabel, TB>(TLabel Label, TB Body)
    : IRegion<TLabel, TB>
{
    public TBR Eval<TBR>(IRegionSemantic<TLabel, TB, TBR> semantic)
        => semantic.Block(Label, Body);

    public IRegion<TLabel, TRR> Select<TRR>(Func<TB, TRR> f)
        => new BlockR<TLabel, TRR>(Label, f(Body));
}

sealed record class LoopR<TLabel, TB>(TLabel Label, TB Body)
    : IRegion<TLabel, TB>
{
    public TBR Eval<TBR>(IRegionSemantic<TLabel, TB, TBR> semantic)
        => semantic.Loop(Label, Body);

    public IRegion<TLabel, TBR> Select<TBR>(Func<TB, TBR> f)
        => new LoopR<TLabel, TBR>(Label, f(Body));
}

sealed record class LabelR<TLabel, TB>(TLabel Label)
    : IRegion<TLabel, TB>
{
    public TBR Eval<TBR>(IRegionSemantic<TLabel, TB, TBR> semantic)
        => semantic.Label(Label);

    public IRegion<TLabel, TBR> Select<TBR>(Func<TB, TBR> f)
        => new LabelR<TLabel, TBR>(Label);
}

sealed record class Region<TLabel, TP>(IRegion<TLabel, SeqPair<Region<TLabel, TP>, TP>> Value)
{
    public Region<TLabel, TR> Select<TR>(Func<TP, TR> f)
        => throw new NotImplementedException();

    public override string ToString()
        => $"[R]({Value})";

    public static Region<TLabel, TP> Block(TLabel label, SeqPair<Region<TLabel, TP>, TP> body)
        => new BlockR<TLabel, SeqPair<Region<TLabel, TP>, TP>>(label, body).Fix();

    public static Region<TLabel, TP> Loop(TLabel label, SeqPair<Region<TLabel, TP>, TP> body)
        => new LoopR<TLabel, SeqPair<Region<TLabel, TP>, TP>>(label, body).Fix();

    public static Region<TLabel, TP> Label(TLabel label) =>
        new LabelR<TLabel, SeqPair<Region<TLabel, TP>, TP>>(label).Fix();

    sealed class FoldSemantic<TBR, TRR>(IRegionFoldSemantic<TLabel, TP, TBR, TRR, TBR, TRR> semantic)
        : IRegionFoldSemantic<TLabel, TP, SeqPair<Region<TLabel, TP>, TP>, Region<TLabel, TP>, TBR, TRR>
    {
        public TRR Block(TLabel label, SeqPair<Region<TLabel, TP>, TP> body)
            => semantic.Block(label, body.Value.Eval(this));

        public TRR Label(TLabel label)
            => semantic.Label(label);

        public TBR Concat(Region<TLabel, TP> region, SeqPair<Region<TLabel, TP>, TP> value)
            => semantic.Concat(region.Value.Eval(this), value.Value.Eval(this));

        public TRR Loop(TLabel label, SeqPair<Region<TLabel, TP>, TP> body)
            => semantic.Loop(label, body.Value.Eval(this));

        public TBR Single(TP value)
            => semantic.Single(value);
    }

    public TR Fold<TBR, TR>(IRegionFoldSemantic<TLabel, TP, TBR, TR, TBR, TR> semantic)
        => Value.Eval(new FoldSemantic<TBR, TR>(semantic));
}

static class RegionExtension
{
    public static Region<TLabel, TP> Fix<TLabel, TP>(this IRegion<TLabel, SeqPair<Region<TLabel, TP>, TP>> x)
        => new(x);

    public static SeqPair<Region<TLabel, TP>, Region<TLabel, TP>> Bind<TLabel, TP>(this Region<TLabel, TP> x) =>
        SeqPair.Concat(x, SeqPair.SingleH(Region<TLabel, TP>.Label(x.Value.Label)));
}

public interface IAtomSemantic<in TI, out TO>
{
    TO LitI(int value);

    TO Add(TI left, TI right);
}

interface IAtom<out T>
{
    TR Evaluate<TR>(IAtomSemantic<T, TR> semantic);
    IAtom<TR> Select<TR>(Func<T, TR> selector);
}

sealed record class Lit<T>(int Value) : IAtom<T>
{
    public R Evaluate<R>(IAtomSemantic<T, R> semantic)
        => semantic.LitI(Value);


    public IAtom<R> Select<R>(Func<T, R> selector)
        => new Lit<R>(Value);

    public override string ToString()
        => Value.ToString();
}

sealed record class Add<T>(T L, T R) : IAtom<T>
{
    public TR Evaluate<TR>(IAtomSemantic<T, TR> semantic)
        => semantic.Add(L, R);


    public IAtom<TR> Select<TR>(Func<T, TR> selector)
        => new Add<TR>(selector(L), selector(R));

    public override string ToString()
        => $"add {L} {R}";
}

interface IExprSemantic<in TI, out TO>
{
    TO Val(TI value);
    TO Exp(IAtom<IExpr<TI>> expr);
}

interface IExpr<out T>
{
    TR Evaluate<TR>(IExprSemantic<T, TR> semantic);
    IExpr<TR> Select<TR>(Func<T, TR> selector);
    IExpr<TR> SelectMany<TR>(Func<T, IExpr<TR>> selector);

    TR Fold<TR>(IAtomSemantic<TR, TR> atom, Func<T, TR> pure);
}

sealed record class Val<TV>(TV Value) : IExpr<TV>
{
    public TR Evaluate<TR>(IExprSemantic<TV, TR> semantic)
        => semantic.Val(Value);

    public IExpr<TR> Select<TR>(Func<TV, TR> selector)
        => new Val<TR>(selector(Value));

    public IExpr<TR> SelectMany<TR>(Func<TV, IExpr<TR>> selector)
        => selector(Value);

    public TR Fold<TR>(IAtomSemantic<TR, TR> atom, Func<TV, TR> pure)
        => pure(Value);

    public override string ToString()
        => $"%{Value}";
}

sealed record class Exp<TV>(IAtom<IExpr<TV>> Expr) : IExpr<TV>
{
    public TR Evaluate<TR>(IExprSemantic<TV, TR> semantic)
        => semantic.Exp(Expr);

    public IExpr<TR> Select<TR>(Func<TV, TR> selector)
        => new Exp<TR>(Expr.Select(e => e.Select(selector)));

    public IExpr<TR> SelectMany<TR>(Func<TV, IExpr<TR>> selector)
        => new Exp<TR>(Expr.Select(e => e.SelectMany(selector)));

    public TR Fold<TR>(IAtomSemantic<TR, TR> atom, Func<TV, TR> pure)
        => Expr.Select(e => e.Fold(atom, pure)).Evaluate(atom);

    public override string ToString()
        => $"({Expr})";
}

interface IJump<out TR, out TV>
{
    TR Region { get; }
    IReadOnlyList<TV> Args { get; }
}

interface ITerminatorSemantic<in TR, in TE, out TO>
{
    TO End();
    TO Ret(TE value);
    TO Br(IJump<TR, TE> target);
    TO BrIf(TE Value, IJump<TR, TE> targetTrue, IJump<TR, TE> targetFalse);
}

interface ITerminator<out TR, out TE>
{
    TO Evaluate<TO>(ITerminatorSemantic<TR, TE, TO> semantic);
    ITerminator<TRR, TER> Select<TRR, TER>(Func<TR, TRR> f, Func<TE, TER> g);
}

sealed record class Ret<TR, TE>(TE Expr)
    : ITerminator<TR, TE>
{
    public TO Evaluate<TO>(ITerminatorSemantic<TR, TE, TO> semantic)
        => semantic.Ret(Expr);

    public ITerminator<TRR, TER> Select<TRR, TER>(Func<TR, TRR> f, Func<TE, TER> g)
        => new Ret<TRR, TER>(g(Expr));

    public override string ToString()
        => $"ret {Expr}";
}

sealed record class Variable(string Name)
{
    public override string ToString()
        => $"${Name}";
}

interface IStmtSemantic<in TV, in TE, out TO>
{
    TO Let(TV val, TE expr);
    TO Load(TV val, Variable variable);
}

interface IStmt<out TV, out TE>
{
    TR Evaluate<TR>(IStmtSemantic<TV, TE, TR> semantic);
    IStmt<TV, TR> Select<TR>(Func<TE, TR> f);
}

sealed record class Let<TV, TE>(TV Val, TE Expr) : IStmt<TV, TE>
{
    public TR Evaluate<TR>(IStmtSemantic<TV, TE, TR> semantic)
        => semantic.Let(Val, Expr);

    public IStmt<TV, TR> Select<TR>(Func<TE, TR> f)
        => new Let<TV, TR>(Val, f(Expr));

    public override string ToString()
        => $"let %{Val} = {Expr}";
}

sealed record class Load<TV, TE>(TV Val, Variable Target) : IStmt<TV, TE>
{
    public TR Evaluate<TR>(IStmtSemantic<TV, TE, TR> semantic)
        => semantic.Load(Val, Target);

    public IStmt<TV, TR> Select<TR>(Func<TE, TR> f)
        => new Load<TV, TR>(Val, Target);

    public override string ToString()
        => $"load %{Val} <- {Target}";
}

// body s t n = t | (s, n)
// fix (body s t) = t | (s, fix (body s t)) 
//                = t | (s, t | (s, fix (body s t)))
//                = t | (s, t | (s, t | (s, fix (body s t))))
//                ~ t | (s, t) | (s, (s, t)) | ...

// pair a b = (a, b)
// body s t = free (pair s) t

// atom t
// expr v = pure v | atom (expr v)

interface IAlgebraS<TRegion, TValue, TExpr, TStmt, TTerm, TSeq>
    : IAtomSemantic<TExpr, TExpr>
    , IStmtSemantic<TValue, TExpr, TStmt>
    , ISeqPairSemantic<TStmt, TTerm, TSeq, TSeq>
    , ITerminatorSemantic<TRegion, TExpr, TTerm>
{
    TExpr Val(TValue v);
}

sealed class ShowAlgebra
    : IAlgebraS<string, int, string, string, string, IEnumerable<string>>
{
    public string LitI(int value)
        => $"{value}";

    public string Add(string left, string right)
        => $"({left} + {right})";

    public string Let(int val, string expr)
        => $"let %{val} = {expr}";

    public string Load(int val, Variable variable)
        => $"%{val} <- {variable}";

    public IEnumerable<string> Single(string value)
        => [value];

    public IEnumerable<string> Concat(string head, IEnumerable<string> tail)
        => [head, ..tail];

    public string End()
        => "end";

    public string Ret(string value)
        => $"ret {value}";

    private string Jump(IJump<string, string> jump)
    {
        var args = string.Join(',', jump.Args);
        return $"^{jump.Region}({args})";
    }

    public string Br(IJump<string, string> target)
        => $"br {Jump(target)}";

    public string BrIf(string Value, IJump<string, string> targetTrue, IJump<string, string> targetFalse)
        => $"brif({Value}, {Jump(targetTrue)}, {Jump(targetFalse)})";

    public string Val(int v)
        => $"%{v}";
}

sealed record class Env<TValue>(
    ImmutableDictionary<Variable, int> Vars,
    ImmutableDictionary<TValue, int> Values
)
    where TValue : notnull
{
    public int this[TValue val]
        => Values[val];

    public int this[Variable v]
        => Vars[v];

    public Env<TValue> Add(TValue v, int value)
        => this with { Values = Values.Add(v, value) };
}

sealed class EvalAlgebra<TValue>
    : IAlgebraS<string, TValue,
        Func<Env<TValue>, int>,
        Func<Env<TValue>, Env<TValue>>,
        Func<Env<TValue>, ITerminator<string, int>>,
        Func<Env<TValue>, ITerminator<string, int>>>
    where TValue : notnull
{
    public Func<Env<TValue>, int> LitI(int value)
        => env => value;

    public Func<Env<TValue>, int> Add(Func<Env<TValue>, int> left, Func<Env<TValue>, int> right)
        => env => left(env) + right(env);

    public Func<Env<TValue>, Env<TValue>> Let(TValue val, Func<Env<TValue>, int> expr)
        => env => env.Add(val, expr(env));

    public Func<Env<TValue>, Env<TValue>> Load(TValue val, Variable variable)
        => env => env.Add(val, env[variable]);

    public Func<Env<TValue>, ITerminator<string, int>> Single(Func<Env<TValue>, ITerminator<string, int>> value)
        => env => value(env);

    public Func<Env<TValue>, ITerminator<string, int>> Concat(Func<Env<TValue>, Env<TValue>> head,
        Func<Env<TValue>, ITerminator<string, int>> tail)
        => env => tail(head(env));

    public Func<Env<TValue>, ITerminator<string, int>> End()
    {
        throw new NotImplementedException();
    }

    public Func<Env<TValue>, ITerminator<string, int>> Ret(Func<Env<TValue>, int> value)
        => env => new Ret<string, int>(value(env));

    public Func<Env<TValue>, ITerminator<string, int>> Br(IJump<string, Func<Env<TValue>, int>> target)
    {
        throw new NotImplementedException();
    }

    public Func<Env<TValue>, ITerminator<string, int>> BrIf(Func<Env<TValue>, int> Value,
        IJump<string, Func<Env<TValue>, int>> targetTrue, IJump<string, Func<Env<TValue>, int>> targetFalse)
    {
        throw new NotImplementedException();
    }

    public Func<Env<TValue>, int> Val(TValue v)
        => env => env[v];
}

static class EvalExtension
{
    sealed class Folder<TRegion, TValue, TExpr, TStmt, TTerm, TSeq>(
        IAlgebraS<TRegion, TValue, TExpr, TStmt, TTerm, TSeq> algebra)
    {
        public TExpr Fold(IExpr<TValue> exp)
            => exp.Fold(algebra, algebra.Val);

        public TStmt Fold(IStmt<TValue, IExpr<TValue>> stmt)
            => stmt.Select(Fold).Evaluate(algebra);

        public TTerm Fold(ITerminator<TRegion, IExpr<TValue>> term)
            => term.Select(x => x, Fold).Evaluate(algebra);

        public TSeq Fold(SeqPair<IStmt<TValue, IExpr<TValue>>, ITerminator<TRegion, IExpr<TValue>>> seq)
        {
            return seq.Value.Select(
                Fold,
                Fold,
                Fold
            ).Eval(algebra);
        }
    }

    public static TSeq Fold<TRegion, TValue, TExpr, TStmt, TTerm, TSeq>(
        this SeqPair<IStmt<TValue, IExpr<TValue>>, ITerminator<TRegion, IExpr<TValue>>> stmt,
        IAlgebraS<TRegion, TValue, TExpr, TStmt, TTerm, TSeq> algebra)
    {
        return new Folder<TRegion, TValue, TExpr, TStmt, TTerm, TSeq>(algebra).Fold(stmt);
    }
}

sealed class FactoryS<TR, TV>(Func<TV> CreateValue)
{
    public IExpr<TV> Lit(int value)
        => new Exp<TV>(new Lit<IExpr<TV>>(value));

    public IExpr<TV> Add(IExpr<TV> l, IExpr<TV> r)
        => new Exp<TV>(new Add<IExpr<TV>>(l, r));

    public IExpr<TV> Val(TV val)
        => new Val<TV>(val);

    public SeqPair<IStmt<TV, IExpr<TV>>, IExpr<TV>> Let(
        IExpr<TV> expr
    )
    {
        var v = CreateValue();
        IStmt<TV, IExpr<TV>> stmt = new Let<TV, IExpr<TV>>(v, expr);
        return SeqPair.Concat(new Let<TV, IExpr<TV>>(v, expr), SeqPair<IStmt<TV, IExpr<TV>>, IExpr<TV>>.Single(Val(v)));
    }

    public SeqPair<IStmt<TV, IExpr<TV>>, IExpr<TV>> Load(
        Variable target
    )
    {
        var v = CreateValue();
        IStmt<TV, IExpr<TV>> stmt = new Load<TV, IExpr<TV>>(v, target);
        return SeqPair.ConcatL(stmt, Val(v));
    }

    public ITerminator<TR, IExpr<TV>> Return(
        IExpr<TV> expr
    ) => new Ret<TR, IExpr<TV>>(expr);
}

static class Test
{
    public static SeqPair<IStmt<TV, IExpr<TV>>, ITerminator<TR, IExpr<TV>>> TestProgram<TR, TV>(FactoryS<TR, TV> b)
        => from x in b.Let(b.Add(b.Lit(1), b.Lit(2)))
           from y in b.Let(b.Add(b.Lit(3), b.Lit(4)))
           from u in b.Load(new Variable("x"))
           from z in b.Let(b.Add(u, x))
           select b.Return(b.Add(x, z));
}