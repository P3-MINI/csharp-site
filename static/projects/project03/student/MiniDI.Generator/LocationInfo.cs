using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MiniDI.Generator;

/// <summary>
/// An equatable replacement for <see cref="Location"/>, which must not be stored in pipeline models
/// (it keeps the whole <see cref="SyntaxTree"/> alive and breaks caching).
/// </summary>
internal sealed record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static LocationInfo? From(Location location) =>
        location.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);

    public static LocationInfo? From(SyntaxNode node) => From(node.GetLocation());

    public static LocationInfo? From(SyntaxToken token) => From(token.GetLocation());

    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
}
