global using Xunit;
global using SigEvalState = SemanticAlgebra.Data.State<System.Collections.Immutable.ImmutableDictionary<LambdaLang.Tests.LambdaLang.Language.Identifier, LambdaLang.Tests.LambdaLang.Language.ISigValue>>;
global using SigEvalData =
    SemanticAlgebra.IS<
        SemanticAlgebra.Data.State<System.Collections.Immutable.ImmutableDictionary<LambdaLang.Tests.LambdaLang.Language.Identifier, LambdaLang.Tests.LambdaLang.Language.ISigValue>>,
        LambdaLang.Tests.LambdaLang.Language.ISigValue
    >;
