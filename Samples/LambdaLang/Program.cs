// See https://aka.ms/new-console-template for more information

using SemanticAlgebra;
using SemanticAlgebra.Option;

var s = Option.Some(40);
var e = s.Select(n => n + 2);
Console.WriteLine(e);
