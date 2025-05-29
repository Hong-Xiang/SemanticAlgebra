using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceGeneratorCore;

internal static class UtilityExtensions
{
    public static string ContainingFullQualifiedNamespace(this ISymbol symbol)
        => symbol.ContainingNamespace.Name;
}
