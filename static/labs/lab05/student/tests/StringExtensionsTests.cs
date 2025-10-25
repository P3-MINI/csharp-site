//#define TASK01
using FluentAssertions;
using tasks;

namespace tests;

#if TASK01
public class StringExtensionsTests
{
    [Theory]
    [InlineData("UserProfileId", "user_profile_id")]
    [InlineData("IsAdminUser", "is_admin_user")]
    [InlineData("EmailAddress", "email_address")]
    [InlineData("HttpRequestHeaders", "http_request_headers")]
    [InlineData("StartDateTime", "start_date_time")]
    [InlineData("XmlParserSettings", "xml_parser_settings")]
    [InlineData("HtmlElementId", "html_element_id")]
    [InlineData("OAuthToken", "o_auth_token")]
    [InlineData("JsonResponseData", "json_response_data")]
    public void PascalToSnakeCase_ShouldConvertCorrectly(string pascalInput, string expectedSnakeOutput)
    {
        // Act
        var result = pascalInput.PascalToSnakeCase();

        // Assert
        result.Should().Be(expectedSnakeOutput);
    }

    [Theory]
    [InlineData("UserProfileId", "user_profile_id")]
    [InlineData("IsAdminUser", "is_admin_user")]
    [InlineData("EmailAddress", "email_address")]
    [InlineData("HttpRequestHeaders", "http_request_headers")]
    [InlineData("StartDateTime", "start_date_time")]
    [InlineData("XmlParserSettings", "xml_parser_settings")]
    [InlineData("HtmlElementId", "html_element_id")]
    [InlineData("OAuthToken", "o_auth_token")]
    [InlineData("JsonResponseData", "json_response_data")]
    public void SnakeToPascalCase_ShouldConvertCorrectly(string expectedPascalOutput, string snakeInput)
    {
        // Act
        var result = snakeInput.SnakeToPascalCase();

        // Assert
        result.Should().Be(expectedPascalOutput);
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    public void PascalToSnakeCase_ShouldReturnEmpty_WhenInputIsNullOrEmpty(string input)
    {
        // Act
        var result = input.PascalToSnakeCase();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    public void SnakeToPascalCase_ShouldReturnEmpty_WhenInputIsNullOrEmpty(string input)
    {
        // Act
        var result = input.SnakeToPascalCase();

        // Assert
        result.Should().BeEmpty();
    }
}
#endif // TASK01