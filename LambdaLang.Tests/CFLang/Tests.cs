using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using Xunit;
using Xunit.Abstractions;

namespace LambdaLang.Tests.CFLang;

public class Tests(ITestOutputHelper Output)
{
    [Fact]
    public void SimpleReturnShouldWork()
    {
        // block():
        //   return 42;
        var S = CfLang.SyntaxFactory;
        var e = S.Block(S.ReturnValue(S.LitI(42)));
        var s = IImplementsM<CfLang, ShowState, string>.Get<StateT<Identity, ShowState>>();
        var (r, _) = e.Fold(s).Run(ShowState.Empty);
        Output.WriteLine(r);
    }

    [Fact]
    public void SimpleLoopShouldWork()
    {
        // C# code
        // public static int SimpleLoop(int x)
        // {
        //    for (var i = 0; i < 300; i++)
        //    {
        //        x += 5;
        //    }
        //    return x;
        // }

        // cil
        /*
         .method public hidebysig static int32
    SimpleLoop(
      int32 x
    ) cil managed
    {
    .custom instance void [DualDrill.CLSL.Language]DualDrill.CLSL.Language.ShaderAttribute.VertexAttribute::.ctor()
      = (01 00 00 00 )
    .maxstack 2
    .locals init (
      [0] int32 i,
      [1] bool V_1,
      [2] int32 V_2
    )

    // [266 5 - 266 6]
    IL_0000: nop

    // [267 14 - 267 23]
    IL_0001: ldc.i4.0
    IL_0002: stloc.0      // i

    IL_0003: br.s         IL_0010
    // start of loop, entry point: IL_0010

      // [268 9 - 268 10]
      IL_0005: nop

      // [269 13 - 269 20]
      IL_0006: ldarg.0      // x
      IL_0007: ldc.i4.5
      IL_0008: add
      IL_0009: starg.s      x

      // [270 9 - 270 10]
      IL_000b: nop

      // [267 34 - 267 37]
      IL_000c: ldloc.0      // i
      IL_000d: ldc.i4.1
      IL_000e: add
      IL_000f: stloc.0      // i

      // [267 25 - 267 32]
      IL_0010: ldloc.0      // i
      IL_0011: ldc.i4       300 // 0x0000012c
      IL_0016: clt
      IL_0018: stloc.1      // V_1

      IL_0019: ldloc.1      // V_1
      IL_001a: brtrue.s     IL_0005
    // end of loop

    // [272 9 - 272 18]
    IL_001c: ldarg.0      // x
    IL_001d: stloc.2      // V_2
    IL_001e: br.s         IL_0020

    // [273 5 - 273 6]
    IL_0020: ldloc.2      // V_2
    IL_0021: ret

    } // end of method DevelopTestShaderModule::SimpleLoop
         */

        // required instructions
        // lit i32 // ldc
        // ldloc/stloc
        // br/br.if
        // ldarg
        // add
        // clt
        // ret

        // for simpliciy, we use i32 to encode bool


        var S = CfLang.SyntaxFactory;

        var locI = new Value("i");
        var locC = new Value("c");
        var locR = new Value("r");
        var argX = new Value("x");


        var lbl05 = new Label("0x05"); // blk05 
        var lbl10 = new Label("0x10"); // loop
        var lbl1c = new Label("0x1c");
        var lbl20 = new Label("0x20");



        var v00t1 = new Value("v00t1");
        var blk00 = S.Block(
          S.LetV(v00t1, S.LitI(0),
          S.SeqV(S.StLoc(locI, S.Val(v00t1)),
          S.Br(S.Reg(lbl10))
        )));

        var v05t1 = new Value("v05t1");
        var v05t2 = new Value("v05t2");
        var blk05 = S.Block(
          S.LetV(v05t1, S.LdArg(argX),
          S.LetV(v05t2, S.Add(S.Val(v05t1), S.LitI(5)),
          S.SeqV(S.StArg(argX, S.Val(v05t2)),
          S.Br(S.Reg(lbl10))
        ))));


        var v10t1 = new Value("v10t1");
        var v10t2 = new Value("v10t2");
        var v10t3 = new Value("v10t3");
        var blk10 = S.Loop(
          S.LetV(v10t1, S.LdLoc(locI),
          S.LetV(v10t2, S.Clt(S.Val(v10t1), S.LitI(300)),
          S.SeqV(S.StLoc(locC, S.Val(v10t2)),
          S.LetV(v10t3, S.LdLoc(locC),
          S.BrIf(v10t3, S.Reg(lbl05), S.Reg(lbl1c))
          )))));


        var v1ct1 = new Value("v1ct1");
        var blk1c = S.Block(
          S.LetV(v1ct1, S.LdArg(argX),
          S.SeqV(S.StLoc(locR, S.Val(v1ct1)),
          S.Br(S.Reg(lbl20)))
        ));


        var v20t1 = new Value("v20t1");
        var blk20 = S.Block(
          S.LetV(v20t1, S.LdLoc(locR),
          S.ReturnValue(S.Val(v20t1))));

        var e =
        S.LetL(lbl10,
          S.LetL(lbl05, blk05,
            S.LetL(lbl1c,
              S.LetL(lbl20, blk20,
              blk1c),
            blk10)),
          blk00
        );

        // let @x = arg i32 in
        // let @i = loc i32 in
        // let @c = loc i32 in
        // let @r = loc i32 in
        // block(): // 0x00
        //    letrec ^10 = loop():
        //       let ^05 = block():
        //          %4 = ldarg @x
        //          %5 = add %4 (lit 5)
        //          starg @x %5
        //          %6 = ldloc @i
        //          %7 = add %6 (lit 1)
        //          stloc @i %7
        //          br ^10 // rec-usage
        //       in
        //       let ^1c = block():
        //          let ^20 = block():
        //             %9 = ldloc @r
        //             ret %9
        //          %8 = ldarg @x
        //          stloc @r %8
        //          br ^20
        //       in
        //       %1 = ldloc %i 
        //       %2 = clt %1 (lit 300)
        //       stloc @c %2
        //       %3 = ldloc @c
        //       brif @c ^05 ^1c
        //    in
        //    %0 = lit 0 
        //    stloc @i %0
        //    br ^10 

        // dominator relation
        // for any let ^x = block()/loop(),
        // x dominates all the blocks that are defined in the body of the block/loop

        // if we allow let to be used in sequence declaration,
        // and ensure all definitions happens at inner most position,
        // e.g. 
        //    let ^x = <region>():
        //        let ^y = <region>():
        //           ... no usage of ^x, and no ^y is y is not loop
        //        ... no usage of ^y
        // should be replaced by
        // let ^x = <region>():
        //       ...
        //     ^y = <region>():
        //       ...

        // then let/letrec ^x = <region>() is isomorphic to dominator tree
    }

    [Fact]
    public void MinimumIfThenElseShouldWork()
    {
        // csharp code
        /*
        public static int MinimumIfThenElse(int a)
        {
            if (a >= 42)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        */

        // cil 
        /*
         .method public hidebysig static int32
    MinimumIfThenElse(
      int32 a
    ) cil managed
  {
    .custom instance void [DualDrill.CLSL.Language]DualDrill.CLSL.Language.ShaderAttribute.VertexAttribute::.ctor()
      = (01 00 00 00 )
    .maxstack 2
    .locals init (
      [0] bool V_0,
      [1] int32 V_1
    )

    // [293 5 - 293 6]
    IL_0000: nop

    // [294 9 - 294 21]
    IL_0001: ldarg.0      // a
    IL_0002: ldc.i4.s     42 // 0x2a
    IL_0004: clt
    IL_0006: ldc.i4.0
    IL_0007: ceq
    IL_0009: stloc.0      // V_0

    IL_000a: ldloc.0      // V_0
    IL_000b: brfalse.s    IL_0012

    // [295 9 - 295 10]
    IL_000d: nop

    // [296 13 - 296 22]
    IL_000e: ldc.i4.1
    IL_000f: stloc.1      // V_1
    IL_0010: br.s         IL_0017

    // [299 9 - 299 10]
    IL_0012: nop

    // [300 13 - 300 22]
    IL_0013: ldc.i4.2
    IL_0014: stloc.1      // V_1
    IL_0015: br.s         IL_0017

    // [302 5 - 302 6]
    IL_0017: ldloc.1      // V_1
    IL_0018: ret

  } // end of method DevelopTestShaderModule::MinimumIfThenElse
         */

        // target ir
        /*
        let @a = arg i32 in
        let @c = loc i32 in
        let @r = loc i32 in
        <e>():
            let ^17 = block():
                %4 = ldloc @r
                ret %4 
            in
            let ^0d = block():
                    %5 = lit 1
                    stloc @r %5
                    br ^17
                ^12 = block():
                    %6 = lit 2
                    stloc @r %6
                    br ^17
            in
            %0 = ldarg @a
            %1 = clt %0 (lit 42)
            %2 = ceq %1 (lit 0)
            stloc @c %2
            %3 = ldloc @c
            brif %3 ^0d ^12
        */
    }
}
