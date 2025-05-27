global using Xunit;
global using SigEvalState = SemanticAlgebra.Data.State<System.Collections.Immutable.ImmutableDictionary<LambdaLang.Language.Identifier, LambdaLang.Language.ISigValue>>;
global using SigEvalData =
    SemanticAlgebra.IS<
        SemanticAlgebra.Data.State<System.Collections.Immutable.ImmutableDictionary<LambdaLang.Language.Identifier, LambdaLang.Language.ISigValue>>,
        LambdaLang.Language.ISigValue
    >;