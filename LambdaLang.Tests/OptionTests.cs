using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using SemanticAlgebra.Free;
using SemanticAlgebra.Option;
using Xunit;

namespace LambdaLang.Tests;

public class OptionTests
{
    [Fact]
    public void Select_WithSome_ReturnsTransformedValue()
    {
        // Arrange
        var s = Option.B.Some(40);

        // Act
        var e = s.Select(n => n + 2);

        // Assert
        Assert.Equal(Option.B.Some(42), e);
    }
    [Fact]
    public void ZipWith_WithTwoSomes_ReturnsCombinedValue()
    {
        // Arrange
        var s = Option.B.Some(40);
        var t = Option.B.Some(2);

        // Act
        var z = s.ZipWith(t, (a, b) => a * b);

        // Assert
        Assert.Equal(Option.B.Some(80), z);
    }
    [Fact]
    public void SelectMany_WithThreeSomes_ReturnsCombinedValue()
    {
        // Arrange & Act
        var x = from a in Option.B.Some(40)
                from b in Option.B.Some(2)
                from c in Option.B.Some(3)
                select a + b + c;

        // Assert
        Assert.Equal(Option.B.Some(45), x);
    }

    sealed class FreeConstantToOption
        : INaturalTransform<SemanticAlgebra.Data.Constant<Unit>, Option>
    {
        public IS<Option, T> Invoke<T>(IS<SemanticAlgebra.Data.Constant<Unit>, T> e)
            => Option.B.None<T>();
    }

    [Fact]
    public void FromConstShouldWorkForAllSome()
    {
        // Arrange & Act
        var x = from a in Free<SemanticAlgebra.Data.Constant<Unit>>.B.Pure(40)
                from b in Free<SemanticAlgebra.Data.Constant<Unit>>.B.Pure(2)
                from c in Free<SemanticAlgebra.Data.Constant<Unit>>.B.Pure(3)
                select a + b + c;
        var v = x.Interprete(new FreeConstantToOption());

        // Assert
        Assert.Equal(Option.B.Some(45), v);
    }

    [Fact]
    public void FromConstShouldWorkForCaseWithNone()
    {
        // Arrange & Act
        var x = from a in Free<Constant<Unit>>.B.Pure(40)
                from b in Constant<Unit>.B.From<int>(Unit.Default).LiftF()
                from c in Free<Constant<Unit>>.B.Pure(3)
                select a + b + c;
        var v = x.Interprete(new FreeConstantToOption());

        // Assert
        Assert.Equal(Option.B.None<int>(), v);
    }

    [Fact]
    public void UnfolderParserTest()
    {
        var unfolder = Fix<Option>.UnFolder<int>(n =>
            n == 0
            ? Option.B.None<int>()
            : Option.B.Some(n - 1)
        );
        var o0 = Option.B.None<Fix<Option>>().Fix();
        var o1 = Option.B.Some(o0).Fix();
        var o2 = Option.B.Some(o1).Fix();
        Assert.Equal(o0, unfolder(0));
        Assert.Equal(o1, unfolder(1));
        Assert.Equal(o2, unfolder(2));
    }

    [Fact]
    public void AddingTopDownAttributeTest()
    {
        var o0 = Option.B.None<Fix<Option>>().Fix();
        var o1 = Option.B.Some(o0).Fix();
        var o2 = Option.B.Some(o1).Fix();


        var o0a = Cofree<Option>.B.From(2, Option.B.None<IS<Cofree<Option>, int>>());
        var o1a = Cofree<Option>.B.From(1, Option.B.Some(o0a));
        var o2a = Cofree<Option>.B.From(0, Option.B.Some(o0a));

        var r = o2.AddAttributeTopDown(0, (x) => x.Expr.Unfix.Select(e => (x.Attr + 1, e)));
        Assert.Equal(o2a, r);
    }

    sealed class AttrIncrOptionSemantic : Option.ISemantic<IS<Cofree<Option>, int>, int>
    {
        public int None()
            => 0;

        public int Some(IS<Cofree<Option>, int> value)
            => value.Unwrap().Head + 1;
    }


    [Fact]
    public void AddingBottomUpAttributeTest()
    {
        var o0 = Option.B.None<Fix<Option>>().Fix();
        var o1 = Option.B.Some(o0).Fix();
        var o2 = Option.B.Some(o1).Fix();


        var o0a = Cofree<Option>.B.From(0, Option.B.None<IS<Cofree<Option>, int>>());
        var o1a = Cofree<Option>.B.From(1, Option.B.Some(o0a));
        var o2a = Cofree<Option>.B.From(2, Option.B.Some(o1a));

        var r = o2.AddAttributeButtomUp<int>((x) => x.Evaluate(new AttrIncrOptionSemantic()));
        Assert.Equal(o2a, r);
    }

    sealed class AttrIncrOptionSemantic2 : Option.ISemantic<int, int>
    {
        public int None()
            => 0;

        public int Some(int value)
            => value + 1;
    }

    sealed class OptionCofreeDistribute : IDistributeTransform<Option, Cofree<Option>>
    {
        sealed class TransformSemantic<T> : Option.ISemantic<IS<Cofree<Option>, T>, IS<Cofree<Option>, IS<Option, T>>>
        {
            public IS<Cofree<Option>, IS<Option, T>> None()
                => Cofree<Option>.B.From(Option.B.None<T>(), Option.B.None<IS<Cofree<Option>, IS<Option, T>>>());

            public IS<Cofree<Option>, IS<Option, T>> Some(IS<Cofree<Option>, T> value)
                => value.Select(Option.B.Some);
        }
        public IS<Cofree<Option>, IS<Option, T>> Distribute<T>(IS<Option, IS<Cofree<Option>, T>> fg)
            => fg.Evaluate(new TransformSemantic<T>());
    }

    [Fact]
    public void AddingBottomUpAttributeTest2()
    {
        var o0 = Option.B.None<Fix<Option>>().Fix();
        var o1 = Option.B.Some(o0).Fix();
        var o2 = Option.B.Some(o1).Fix();


        var o0a = Cofree<Option>.B.From(0, Option.B.None<IS<Cofree<Option>, int>>());
        var o1a = Cofree<Option>.B.From(1, Option.B.Some(o0a));
        var o2a = Cofree<Option>.B.From(2, Option.B.Some(o1a));

        var r = o2.AddAttributeButtomUp2<int>(new OptionCofreeDistribute(), (x) => x.Evaluate(new AttrIncrOptionSemantic2()));
        Assert.Equal(o2a, r);
    }

}