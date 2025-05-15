using SemanticAlgebra;
using SemanticAlgebra.Option;


var s = Option.Some(40);
var e = s.Select(n => n + 2);
Console.WriteLine(e);
var t = Option.Some(2);
var z = s.ZipWith(t, (a, b) => a * b);
Console.WriteLine(z);
var x = from a in s
        from b in Option.Some(2)
        from c in Option.Some(3)
        select a + b + c;
Console.WriteLine(x);