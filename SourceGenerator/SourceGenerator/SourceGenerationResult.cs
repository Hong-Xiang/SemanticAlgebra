using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Web;

namespace SourceGeneratorCore;

public interface ISourceGenerationResult
{
    void Generate(SourceProductionContext context);
}

public sealed record class SourceCodeFile(
    string FileName,
    StringBuilder StringBuilder
) : ISourceGenerationResult
{
    public void Generate(SourceProductionContext context)
    {
        var escapedFileName = HttpUtility.UrlEncode(FileName).Replace('%', '_');
        context.AddSource($"{escapedFileName}.g.cs", StringBuilder.ToString());
    }
}
