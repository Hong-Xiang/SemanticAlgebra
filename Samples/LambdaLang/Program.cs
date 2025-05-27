using LambdaLang;
using LambdaLang.Language;
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Option;
using System.Collections.Immutable;


// var s = Option.Some(40);
// var e = s.Select(n => n + 2);
// Console.WriteLine(e);
// var t = Option.Some(2);
// var z = s.ZipWith(t, (a, b) => a * b);
// Console.WriteLine(z);
// var x = from a in s
//         from b in Option.Some(2)
//         from c in Option.Some(3)
//         select a + b + c;
// Console.WriteLine(x);

//var sf = IntLang.SyntaxFactory;
//var fe = sf.LitI(40);
//var fadd = sf.Add(fe, fe);

//var fv = fadd.Fold(new IntFolder());
//Console.WriteLine(fv);

var S = Fix<Sig>.SyntaxFactory.Prj();

var x = new Identifier("x");
var v = new Identifier("v");
var add1 = S.Lambda(x, S.Add(S.LitI(1), S.Var(x)));

var f = new Identifier("f");
var x2 = new Identifier("x");
var z = S.Lambda(f, S.Lambda(x2, S.Var(x2)));




var f2 = new Identifier("f");
var n = new Identifier("n");
var x3 = new Identifier("x");
var succ = S.Lambda(n, S.Lambda(f2, S.Lambda(x3,
   S.Apply(S.Var(f2), 
        S.Apply(
            S.Apply(S.Var(n), S.Var(f2)),
            S.Var(x3)))
    )));

var c1 = S.Apply(succ, z);
var c2 = S.Apply(succ, c1);
var c3 = S.Apply(succ, c2);

//var expr = S.Let(v, S.LitI(41), S.Apply(add1, S.Var(v)));
//var expr = S.Apply(add1, S.LitI(41));

var l41 = S.LitI(41);

//var expr = S.Apply(S.Apply(z, add1), l41);
var expr = S.Apply(S.Apply(c3, add1), l41);


var show = expr.Fold<string>(Sig.SigSemantic(
    new LitShowFolder(),
    new ArithShowFolder(),
    new LamShowFolder(),
    new AppShowFolder(),
    new BindShowFolder()));
Console.WriteLine(show);

var vals = expr.Fold<SigEvalData>(Sig.SigSemantic(
    new LitEvalFolder(),
    new ArithEvalFolder(),
    new LamEvalFolder(),
    new AppEvalFolder(),
    new BindEvalFolder()));
var valr = vals.Run(ImmutableDictionary<Identifier, ISigValue>.Empty);
Console.WriteLine(valr.Data);
