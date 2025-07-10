using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExprFTest;


//sealed record class Let<TV, T>(T Value, Func<TV, T> Next) : IExpr<T>
//{
//    public TR Evaluate<V, TR>(IExprSemantic<T, V, TR> semantic)
//        => semantic.Let(Value, Next); // compile error here on Next

//    public IExpr<TR> Select<TR>(Func<T, TR> selector)
//    {
//        return new Let<TV, TR>(selector(Value), Next); // compile error here on Next
//    }
//}

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
// term e r = end | ret e | jmp r e[]
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

interface INamedRegionSemantic<in TLabel, in TValue, in TBody, out TResult>
{
    TResult Block(TLabel label, IReadOnlyList<TValue> args, TBody body);
    TResult Loop(TLabel label, IReadOnlyList<TValue> args, TBody body);

    //Func<INamedRegion<TLabel, TValue, TBody>, TResult> ToFunc()
        //=> e => e.Evaluate(this);
}

interface INamedRegion<out TLabel, out TValue, out TBody>
{
    TLabel Label { get; }
    IReadOnlyList<TValue> Args { get; }
    TBody Body { get; }
    INamedRegion<TLabel, TValue, TResult> Select<TResult>(Func<TBody, TResult> f);
    INamedRegion<TLR, TVR, TBR> Select<TLR, TVR, TBR>(Func<TLabel, TLR> fl, Func<TValue, TVR> fv, Func<TBody, TBR> fb);
    TResult Evaluate<TResult>(INamedRegionSemantic<TLabel, TValue, TBody, TResult> semantic);
}

sealed record class Block<TLabel, TValue, TBody>(TLabel Label, IReadOnlyList<TValue> Args, TBody Body)
    : INamedRegion<TLabel, TValue, TBody>
{
    public TResult Evaluate<TResult>(INamedRegionSemantic<TLabel, TValue, TBody, TResult> semantic)
        => semantic.Block(Label, Args, Body);

    public INamedRegion<TLabel, TValue, TResult> Select<TResult>(Func<TBody, TResult> f)
        => new Block<TLabel, TValue, TResult>(Label, Args, f(Body));

    public INamedRegion<TLR, TVR, TBR> Select<TLR, TVR, TBR>(Func<TLabel, TLR> fl, Func<TValue, TVR> fv, Func<TBody, TBR> fb)
        => new Block<TLR, TVR, TBR>(fl(Label), [.. Args.Select(fv)], fb(Body));
}

sealed record class Loop<TLabel, TValue, TBody>(TLabel Label, IReadOnlyList<TValue> Args, TBody Body)
    : INamedRegion<TLabel, TValue, TBody>
{
    public TResult Evaluate<TResult>(INamedRegionSemantic<TLabel, TValue, TBody, TResult> semantic)
        => semantic.Loop(Label, Args, Body);

    public INamedRegion<TLabel, TValue, TResult> Select<TResult>(Func<TBody, TResult> f)
        => new Loop<TLabel, TValue, TResult>(Label, Args, f(Body));

    public INamedRegion<TLR, TVR, TBR> Select<TLR, TVR, TBR>(Func<TLabel, TLR> fl, Func<TValue, TVR> fv, Func<TBody, TBR> fb)
        => new Loop<TLR, TVR, TBR>(fl(Label), [.. Args.Select(fv)], fb(Body));
}

interface IBasicBlock<out TLabel, out TValue, out TExpr, out TRegion>
{
}

// free (named-region l v) x
interface IRegionBodySemantic<in TLable, in TValue, in TI, out TO>
{
    TO Pure(TI value);
    TO Nest(INamedRegion<TLable, TValue, IRegionBody<TLable, TValue, TI>> region);
}

interface IRegionBody<out TLabel, out TValue, out T>
{
    TResult Evaluate<TResult>(IRegionBodySemantic<TLabel, TValue, T, TResult> semantic);
}

interface ISwitchCase<out TRegion>
{
    int Case { get; }
    TRegion Target { get; }
}

interface IControlFlowSemantic<in TLabel, in TValue, in TExpr, in TRegionBody, out TResult>
{
    TResult Named(TLabel label, TRegionBody region);
    TResult If(TExpr value, TRegionBody then, TRegionBody @else);
    TResult Switch(TExpr value, IReadOnlyList<ISwitchCase<TRegionBody>> cases);
}

interface IControlFlow<out TLabel, out TValue, out TExpr, out TRegionBody>
{
    TResult Evaluate<TResult>(IControlFlowSemantic<TLabel, TValue, TExpr, TRegionBody, TResult> semantic);
}

// region-body l v basic-block
interface IRegionSemantic<in TLabel, in TValue, in TExpr, in TRegion, out TResult>
{
    TResult Region(IControlFlow<TLabel, TValue, TExpr, IRegionBody<TLabel, TValue, IBasicBlock<TLabel, TValue, TExpr, TRegion>>> controlFlow);
    TResult Label(TLabel label);
}

interface IRegion<out TLabel, out TValue, out TExpr, out TRegion>
{
    TResult Evaluate<TResult>(IRegionSemantic<TLabel, TValue, TExpr, TRegion, TResult> semantic);
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
    TR Evaluate<V, TR>(IExprSemantic<T, TR> semantic);
    IExpr<TR> Select<TR>(Func<T, TR> selector);
    IExpr<TR> SelectMany<TR>(Func<T, IExpr<TR>> selector);
}

sealed record class Val<TV>(TV Value) : IExpr<TV>
{
    public TR Evaluate<V, TR>(IExprSemantic<TV, TR> semantic)
        => semantic.Val(Value);

    public IExpr<TR> Select<TR>(Func<TV, TR> selector)
        => new Val<TR>(selector(Value));

    public IExpr<TR> SelectMany<TR>(Func<TV, IExpr<TR>> selector)
        => selector(Value);

    public override string ToString()
        => $"%{Value}";
}

sealed record class Exp<TV>(IAtom<IExpr<TV>> Expr) : IExpr<TV>
{
    public TR Evaluate<V, TR>(IExprSemantic<TV, TR> semantic)
        => semantic.Exp(Expr);

    public IExpr<TR> Select<TR>(Func<TV, TR> selector)
        => new Exp<TR>(Expr.Select(e => e.Select(selector)));

    public IExpr<TR> SelectMany<TR>(Func<TV, IExpr<TR>> selector)
        => new Exp<TR>(Expr.Select(e => e.SelectMany(selector)));

    public override string ToString()
        => $"({Expr})";
}

sealed record class Ret<T>(T? Expr)
{
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

interface IPairSemantic<in TS, in TN, out TO>
{
    TO Pair(TS stmt, TN next);
}

interface IPair<out TS, out TN>
{
    TR Evaluate<TR>(IPairSemantic<TS, TN, TR> semantic);
    IPair<TS, TR> Select<TR>(Func<TN, TR> f);
    IPair<TSR, TNR> Select<TSR, TNR>(Func<TS, TSR> fs, Func<TN, TNR> fn);
}

sealed record class Pair<S, N>(S Stmt, N Next) : IPair<S, N>
{
    public TR Evaluate<TR>(IPairSemantic<S, N, TR> semantic)
        => semantic.Pair(Stmt, Next);

    public IPair<S, TR> Select<TR>(Func<N, TR> f)
        => new Pair<S, TR>(Stmt, f(Next));

    public IPair<TSR, TNR> Select<TSR, TNR>(Func<S, TSR> fs, Func<N, TNR> fn)
        => new Pair<TSR, TNR>(fs(Stmt), fn(Next));
}

interface IComp2Semantic<TS, TI, out TO>
{
    TO Pure(TI value);
    TO Free(Pair<TS, IComp2<TS, TI>> pair);
}

interface IComp2<TS, T>
{
    TR Evaluate<TR>(IComp2Semantic<TS, T, TR> semantic);
    IComp2<TS, TR> Select<TR>(Func<T, TR> f);
    IComp2<TS, TR> SelectMany<TR>(Func<T, IComp2<TS, TR>> f);
}

sealed record class Pure2<TS, T>(T Ret) : IComp2<TS, T>
{
    public TR Evaluate<TR>(IComp2Semantic<TS, T, TR> semantic)
        => semantic.Pure(Ret);

    public IComp2<TS, TR> Select<TR>(Func<T, TR> f)
        => new Pure2<TS, TR>(f(Ret));

    public IComp2<TS, TR> SelectMany<TR>(Func<T, IComp2<TS, TR>> f)
        => f(Ret);

    public override string ToString()
        => $"[T]{Ret}";
}

sealed record class Comp2<TS, T>(Pair<TS, IComp2<TS, T>> Pair) : IComp2<TS, T>
{
    public TR Evaluate<TR>(IComp2Semantic<TS, T, TR> semantic)
        => semantic.Free(Pair);

    public IComp2<TS, TR> Select<TR>(Func<T, TR> f)
    {
        var p = Pair.Select(c => c.Select(f));
        return new Comp2<TS, TR>((Pair<TS, IComp2<TS, TR>>)p);
    }

    public IComp2<TS, TR> SelectMany<TR>(Func<T, IComp2<TS, TR>> f)
        => new Comp2<TS, TR>((Pair<TS, IComp2<TS, TR>>)Pair.Select(c => c.SelectMany(f)));

    public override string ToString()
        => $"[C]{Pair.Stmt};{Environment.NewLine}{Pair.Next}";
}

interface ICompSemantic<TS, in TI, out TO>
{
    TO Pure(TI terminator);
    TO Comp(TS stmt, IComp<TS, TI> next);
}

interface IComp<TS, out T>
{
    public TR Evaluate<TR>(ICompSemantic<TS, T, TR> semantic);
    public IComp<TS, TR> Select<TR>(Func<T, TR> f);
    public IComp<TS, TR> SelectMany<TR>(Func<T, IComp<TS, TR>> f);
}

sealed record class Pure<TS, T>(T Ret) : IComp<TS, T>
{
    public TR Evaluate<TR>(ICompSemantic<TS, T, TR> semantic)
        => semantic.Pure(Ret);

    public IComp<TS, TR> Select<TR>(Func<T, TR> f)
        => new Pure<TS, TR>(f(Ret));

    public IComp<TS, TR> SelectMany<TR>(Func<T, IComp<TS, TR>> f)
        => f(Ret);

    public override string ToString()
        => $"[T]{Ret}";
}

sealed record class Comp<TS, T>(TS Stmt, IComp<TS, T> Next) : IComp<TS, T>
{
    public TR Evaluate<TR>(ICompSemantic<TS, T, TR> semantic)
        => semantic.Comp(Stmt, Next);

    public IComp<TS, TR> Select<TR>(Func<T, TR> f)
        => new Comp<TS, TR>(Stmt, Next.Select(f));

    public IComp<TS, TR> SelectMany<TR>(Func<T, IComp<TS, TR>> f)
        => new Comp<TS, TR>(Stmt, Next.SelectMany(f));

    public override string ToString()
        => $"[C]{Stmt};{Environment.NewLine}{Next}";
}

sealed class Factory<TV>(Func<TV> CreateValue)
{
    public IExpr<TV> Lit(int value)
        => new Exp<TV>(new Lit<IExpr<TV>>(value));
    public IExpr<TV> Add(IExpr<TV> l, IExpr<TV> r)
        => new Exp<TV>(new Add<IExpr<TV>>(l, r));
    public IExpr<TV> Val(TV val)
        => new Val<TV>(val);

    public IComp<IStmt<TV, IExpr<TV>>, IExpr<TV>> Let(
        IExpr<TV> expr
    )
    {
        var v = CreateValue();
        return new Comp<IStmt<TV, IExpr<TV>>, IExpr<TV>>(
            new Let<TV, IExpr<TV>>(v, expr),
            new Pure<IStmt<TV, IExpr<TV>>, IExpr<TV>>(Val(v))
        );
    }

    public IComp<IStmt<TV, IExpr<TV>>, IExpr<TV>> Load(
        Variable target
    )
    {
        var v = CreateValue();
        return new Comp<IStmt<TV, IExpr<TV>>, IExpr<TV>>(
            new Load<TV, IExpr<TV>>(v, target),
            new Pure<IStmt<TV, IExpr<TV>>, IExpr<TV>>(Val(v))
        );
    }


    public Ret<IExpr<TV>> Return(
        IExpr<TV> expr
    ) => new Ret<IExpr<TV>>(expr);
}

sealed class Factory2<TV>(Func<TV> CreateValue)
{
    public IExpr<TV> Lit(int value)
        => new Exp<TV>(new Lit<IExpr<TV>>(value));
    public IExpr<TV> Add(IExpr<TV> l, IExpr<TV> r)
        => new Exp<TV>(new Add<IExpr<TV>>(l, r));
    public IExpr<TV> Val(TV val)
        => new Val<TV>(val);

    public IComp2<IStmt<TV, IExpr<TV>>, IExpr<TV>> Let(
        IExpr<TV> expr
    )
    {
        var v = CreateValue();
        return Test.Pair(
            new Let<TV, IExpr<TV>>(v, expr),
            new Pure2<IStmt<TV, IExpr<TV>>, IExpr<TV>>(Val(v))
        ).Lift();
    }

    public IComp2<IStmt<TV, IExpr<TV>>, IExpr<TV>> Load(
        Variable target
    )
    {
        var v = CreateValue();
        return Test.Pair(
            new Load<TV, IExpr<TV>>(v, target),
            new Pure2<IStmt<TV, IExpr<TV>>, IExpr<TV>>(Val(v))
        ).Lift();
    }


    public Ret<IExpr<TV>> Return(
        IExpr<TV> expr
    ) => new Ret<IExpr<TV>>(expr);
}




static class Test
{
    public static IPair<S, N> Pair<S, N>(S s, N n) => new Pair<S, N>(s, n);

    public static IComp2<TS, T> Lift<TS, T>(this IPair<TS, IComp2<TS, T>> pair)
        => new Comp2<TS, T>((Pair<TS, IComp2<TS, T>>)pair);

    public static IComp<TStmt, TR> SelectMany<TStmt, TS, TM, TR>(
        this IComp<TStmt, TS> source,
        Func<TS, IComp<TStmt, TM>> collectionSelector,
        Func<TS, TM, TR> resultSelector)
         => source.SelectMany(s => collectionSelector(s).Select(m => resultSelector(s, m)));

    public static IComp2<TStmt, TR> SelectMany<TStmt, TS, TM, TR>(
           this IComp2<TStmt, TS> source,
           Func<TS, IComp2<TStmt, TM>> collectionSelector,
           Func<TS, TM, TR> resultSelector)
            => source.SelectMany(s => collectionSelector(s).Select(m => resultSelector(s, m)));



    public static IComp<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> TestProgram<TV>(Factory<TV> b)
           => from x in b.Let(b.Add(b.Lit(1), b.Lit(2))) // desired syntax corrected to use let
              from y in b.Let(b.Add(b.Lit(3), b.Lit(4)))
              from u in b.Load(new Variable("x"))
              from z in b.Let(b.Add(u, x))
              select b.Return(b.Add(x, z));

    public static IComp<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> TestProgram2<TV>(Factory<TV> b)
           => from x in b.Let(b.Add(b.Lit(1), b.Lit(2))) // desired syntax corrected to use let
              from y in b.Let(b.Add(b.Lit(3), b.Lit(4)))
              from u in b.Load(new Variable("x"))
              from z in b.Let(b.Add(u, x))
              select b.Return(b.Add(x, z));



    public static T Evaluate<TV, T>(
        this IComp<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> prog,
        IAlgebra<TV, T> algebra)
        => prog switch
        {
            Pure<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> pure =>
                algebra.Pure(pure.Ret.Expr.Evaluate(algebra)),
            Comp<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> comp =>
                comp.Next.Evaluate(comp.Stmt.Select(e => e.Evaluate(algebra)).Evaluate(algebra)),
            _ => throw new InvalidOperationException()
        };

    public static T Evaluate<TV, T>(
        this IExpr<TV> expr,
        IAlgebra<TV, T> algebra)
        => expr switch
        {
            Val<TV> val => algebra.Val(val.Value),
            Exp<TV> nest => nest.Expr.Select(e => e.Evaluate(algebra)).Evaluate(algebra),
            _ => throw new InvalidOperationException()
        };

    //public static T Evaluate2<TV, T>(
    //       this IComp2<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> prog,
    //       IAlgebra<TV, T> algebra)
    //       => prog switch
    //       {
    //           Pure2<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> pure =>
    //               algebra.Pure(pure.Ret.Expr.Evaluate(algebra)),
    //           Comp2<IStmt<TV, IExpr<TV>>, Ret<IExpr<TV>>> comp =>
    //               comp.Pair.Stmt .Evaluate(comp.Stmt.Select(e => e.Evaluate(algebra)).Evaluate(algebra)),
    //           _ => throw new InvalidOperationException()
    //       };

    //public static T Evaluate2<TV, T>(
    //    this IExpr<TV> expr,
    //    IAlgebra<TV, T> algebra)
    //    => expr switch
    //    {
    //        Val<TV> val => algebra.Val(val.Value),
    //        Exp<TV> nest => nest.Expr.Select(e => e.Evaluate(algebra)).Evaluate(algebra),
    //        _ => throw new InvalidOperationException()
    //    };

}

interface IAlgebra<TV, T>
    : IStmtSemantic<TV, T, IAlgebra<TV, T>>
    , IAtomSemantic<T, T>
{
    T Val(TV value);
    T Pure(T value);
}

sealed record class SimpleEvalAlgebra<TV>(
    IReadOnlyDictionary<string, int> Vars,
    ImmutableDictionary<TV, int> Env
)
    : IAlgebra<TV, int>
    where TV : notnull
{
    public int Add(int left, int right)
        => left + right;

    public IAlgebra<TV, int> Let(TV val, int expr)
        => this with { Env = Env.Add(val, expr) };

    public int LitI(int value)
        => value;

    public IAlgebra<TV, int> Load(TV val, Variable variable)
        => this with { Env = Env.Add(val, Vars[variable.Name]) };

    public int Pure(int terminator)
        => terminator;

    public int Val(TV value)
        => Env[value];
}
