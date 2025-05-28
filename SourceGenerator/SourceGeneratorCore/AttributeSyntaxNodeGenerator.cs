using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SourceGeneratorCore;

interface IAttributeSyntaxNodeGeneratorFactory
{
    string AttributeName { get; }
    bool IsTarget(SyntaxNode node, CancellationToken cancellation);
}

interface AttributeSyntaxNodeGenerator
{
}
