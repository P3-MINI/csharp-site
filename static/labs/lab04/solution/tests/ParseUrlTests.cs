using FluentAssertions;
using tasks;

namespace tests;

public class ParseUrlTests
{
    [Fact]
    public void ParseUrl_ValidFullUrl_ShouldReturnSuccessAndCorrectParsedUrl()
    {
        // Arrange
        var url = "https://example.com/v2/users/42/orders/100?status=paid&status=shipped&sort=date";

        // Act
        var (parsed, status) = Task01.ParseUrl(url);

        // Assert
        status.Should().Be(ParsingStatus.Success);
        parsed.Scheme.Should().Be(UrlScheme.Https);
        parsed.Host.Should().Be("example.com");
        parsed.Version.Should().Be(2);

        parsed.PathSegments.Should().HaveCount(2);
        parsed.PathSegments[0].Name.Should().Be("users");
        parsed.PathSegments[0].Id.Should().Be(42);
        parsed.PathSegments[1].Name.Should().Be("orders");
        parsed.PathSegments[1].Id.Should().Be(100);

        parsed.QueryParams.Should().ContainKey("status")
            .WhoseValue.Should().BeEquivalentTo("paid", "shipped");
        parsed.QueryParams.Should().ContainKey("sort")
            .WhoseValue.Should().BeEquivalentTo("date");
    }

    [Theory]
    [InlineData("smtp://host/v1/x/1", ParsingStatus.InvalidScheme)]
    [InlineData("http:///v1/x/1", ParsingStatus.InvalidHost)]
    [InlineData("http://host/v0/x/1", ParsingStatus.InvalidVersion)]
    [InlineData("http://host/vx/x/1", ParsingStatus.InvalidVersion)]
    [InlineData("http://host/v1/users/42/orders", ParsingStatus.InvalidPath)]
    [InlineData("http://host/v1/users/abc", ParsingStatus.InvalidId)]
    [InlineData("http://host/v1/users/-5", ParsingStatus.InvalidId)]
    [InlineData("http://host/v1/x/1?foo", ParsingStatus.InvalidQuery)]
    [InlineData("http://host/1/users/1", ParsingStatus.InvalidVersion)]
    public void ParseUrl_InvalidInputs_ShouldReturnExpectedStatus(string url, ParsingStatus expected)
    {
        // Act
        var (_, status) = Task01.ParseUrl(url);

        // Assert
        status.Should().Be(expected);
    }
}