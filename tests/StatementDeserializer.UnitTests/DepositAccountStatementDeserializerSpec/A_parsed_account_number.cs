using FluentAssertions;
using StatementDeserializer.UnitTests.Resources.Content.DepositAccounts;
using System;
using Xunit;

namespace StatementDeserializer.UnitTests.DepositAccountStatementDeserializerSpec
{
    public class A_parsed_account_number : IClassFixture<TsvFixture>
    {
        private readonly TsvFixture _fixture;

        public A_parsed_account_number(TsvFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void identifies_account_number_correctly()
        {
            // +Arrange
            const string expectedNumber = "12-3085-0000000-00";

            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.CardOrAccountNumber.Should().NotBeNull();
            _fixture.Snapshot.CardOrAccountNumber.Should().Be(expectedNumber);
        }

        [Fact]
        public void throws_format_exception_when_bank_is_not_numeric()
        {
            // +Arrange
            var sut = new DepositAccountStatementDeserializer();

            // +Act
            var exception = Assert.Throws<FormatException>(() => sut.DeserializeTdv(SampleDepositAccountReports.TdvAccountMalformedBank));

            // +Assert
            exception.Message.Should().Be("Expected a numeric bank identifier; given 'oops'.");
        }

        [Fact]
        public void throws_format_exception_when_branch_is_not_numeric()
        {
            // +Arrange
            var sut = new DepositAccountStatementDeserializer();

            // +Act
            var exception = Assert.Throws<FormatException>(() => sut.DeserializeTdv(SampleDepositAccountReports.TdvAccountMalformedBranch));

            // +Assert
            exception.Message.Should().Be("Expected a numeric branch identifier; given 'oops'.");
        }
    }
}