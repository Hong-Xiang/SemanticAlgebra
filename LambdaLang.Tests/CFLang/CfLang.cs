using SemanticAlgebra;
using SemanticAlgebra.Data;
using SemanticAlgebra.Fix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaLang.Tests.CFLang;

public partial interface CfLang
    : IFunctor<CfLang>
    , IMergedSemantic1<CfLang, Scf, Ter, Lit, Bind, Arith, Ref>
    , Scf, Ter, Lit, Bind, Arith, Ref
    , IImplementsM<CfLang, ShowState, string>
{

    static ISemantic1<CfLang, TS, TR> IMergedSemantic1<CfLang, Scf, Ter, Lit, Bind, Arith, Ref>.MergeSemantic<TS, TR>(
        ISemantic1<Scf, TS, TR> s1,
        ISemantic1<Ter, TS, TR> s2,
        ISemantic1<Lit, TS, TR> s3,
        ISemantic1<Bind, TS, TR> s4,
        ISemantic1<Arith, TS, TR> s5,
        ISemantic1<Ref, TS, TR> s6)
        => CreateMergeSemantic<TS, TR>(s1, s2, s3, s4, s5, s6);

    static ISemantic1<CfLang, TS, TR> CreateMergeSemantic<TS, TR>(
        ISemantic1<Scf, TS, TR> s1,
        ISemantic1<Ter, TS, TR> s2,
        ISemantic1<Lit, TS, TR> s3,
        ISemantic1<Bind, TS, TR> s4,
        ISemantic1<Arith, TS, TR> s5,
        ISemantic1<Ref, TS, TR> s6
        )
        => new MergedSemantic<TS, TR>(s1.Prj(), s2.Prj(), s3.Prj(), s4.Prj(), s5.Prj(), s6.Prj());

    public interface IMergedSemantic<in TS, out TR>
        : ISemantic1<CfLang, TS, TR>
        , Scf.ISemantic<TS, TR>
        , Ter.ISemantic<TS, TR>
        , Lit.ISemantic<TS, TR>
        , Bind.ISemantic<TS, TR>
        , Arith.ISemantic<TS, TR>
        , Ref.ISemantic<TS, TR>
    {
    }


    static ISemantic1<CfLang,
                 IS<M, string>,
                 IS<M, string>>
             IImplementsM<CfLang, ShowState, string>.GetS<M>()
             => CreateMergeSemantic(
                 IImplementsM<Scf, ShowState, string>.Get<M>(),
                 IImplementsM<Ter, ShowState, string>.Get<M>(),
                 IImplementsM<Lit, ShowState, string>.Get<M>(),
                 IImplementsM<Bind, ShowState, string>.Get<M>(),
                 IImplementsM<Arith, ShowState, string>.Get<M>(),
                 IImplementsM<Ref, ShowState, string>.Get<M>()
             );


    public sealed class MergedSemantic<TS, TR>(
        Scf.ISemantic<TS, TR> Scf,
        Ter.ISemantic<TS, TR> Ter,
        Lit.ISemantic<TS, TR> Lit,
        Bind.ISemantic<TS, TR> Eff,
        Arith.ISemantic<TS, TR> Ari,
        Ref.ISemantic<TS, TR> Ref
    )
        : IMergedSemantic<TS, TR>
    {
        public TR Add(TS a, TS b)
            => Ari.Add(a, b);

        public TR Reg(Label name)
            => Eff.Reg(name);

        public TR Block(TS body)
            => Scf.Block(body);

        public TR Br(TS label)
            => Ter.Br(label);

        public TR BrIf(Value value, TS trueLabel, TS falseLabel)
            => Ter.BrIf(value, trueLabel, falseLabel);


        public TR Clt(TS a, TS b)
            => Ari.Clt(a, b);

        public TR LdArg(Value name)
            => Ref.LdArg(name);

        public TR LdLoc(Value name)
            => Ref.LdLoc(name);

        public TR LetL(Label name, TS value, TS next)
            => Eff.LetL(name, value, next);

        public TR LetV(Value name, TS value, TS next)
            => Eff.LetV(name, value, next);

        public TR LitI(int value)
            => Lit.LitI(value);

        public TR Loop(TS body)
            => Scf.Loop(body);

        public TR Return()
            => Ter.Return();

        public TR ReturnValue(TS val)
            => Ter.ReturnValue(val);

        public TR SeqV(TS step, TS next)
            => Eff.SeqV(step, next);

        public TR StArg(Value name, TS value)
            => Ref.StArg(name, value);

        public TR StLoc(Value name, TS value)
            => Ref.StLoc(name, value);

        public TR Val(Value name)
            => Eff.Val(name);

        public TR Ceq(TS a, TS b)
            => Ari.Ceq(a, b);
    }

    public static IMergedSemantic<Fix<CfLang>, Fix<CfLang>> SyntaxFactory { get; }
        = (IMergedSemantic<Fix<CfLang>, Fix<CfLang>>)Fix<CfLang>.SyntaxFactory;
}




