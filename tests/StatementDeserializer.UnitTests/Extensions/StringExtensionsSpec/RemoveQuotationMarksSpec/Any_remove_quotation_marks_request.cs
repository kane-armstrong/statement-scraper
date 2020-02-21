using FluentAssertions;
using StatementDeserializer.Extensions;
using Xunit;

namespace StatementDeserializer.UnitTests.Extensions.StringExtensionsSpec.RemoveQuotationMarksSpec
{
    public class Any_remove_quotation_marks_request
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("no quotes to remove", "no quotes to remove")]
        [InlineData("\"has quotes to remove\"", "has quotes to remove")]
        public void produces_expected_result(string text, string expected)
        {
            // +Act
            var result = text.RemoveQuotationMarks();

            // +Assert
            result.Should().Be(expected);
        }
    }
}