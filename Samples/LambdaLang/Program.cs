using LambdaLang;
using LambdaLang.Language;
using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Option;
using System.Collections.Immutable;

// Demonstrate the Source Generator
Console.WriteLine("=== Source Generator Demo ===");
var testInstance = new TestClass();
testInstance.TestMethod();
Console.WriteLine();

Console.WriteLine("=== Lambda Language Demo ===");

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

var l41 = S.LitI(41);
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
