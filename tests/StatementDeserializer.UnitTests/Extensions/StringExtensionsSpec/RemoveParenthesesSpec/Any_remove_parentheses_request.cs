using FluentAssertions;
using Xunit;
using StatementDeserializer.Extensions;

namespace StatementDeserializer.UnitTests.Extensions.StringExtensionsSpec.RemoveParenthesesSpec
{
    public class Any_remove_parentheses_request
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("no parentheses to remove", "no parentheses to remove")]
        [InlineData("(has parentheses to remove)", "has parentheses to remove")]
        public void produces_expected_result(string text, string expected)
        {
            // +Act
            var result = text.RemoveParentheses();

            // +Assert
            result.Should().Be(expected);
        }
    }
}