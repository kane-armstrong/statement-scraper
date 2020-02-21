using FluentAssertions;
using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace StatementDeserializer.UnitTests.CardAccountStatementDeserializerSpec
{
    public class A_parsed_set_of_transactions : IClassFixture<TsvFixture>
    {
        private readonly TsvFixture _fixture;

        public A_parsed_set_of_transactions(TsvFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void identifies_correct_number_of_transactions()
        {
            // +Arrange
            const int expectedNumberOfTransactions = 7;

            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Count.Should().Be(expectedNumberOfTransactions);
        }

        [Fact]
        public void does_not_contain_any_silent_parsing_failures()
        {
            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Should().NotContainNulls();
        }

        private static readonly DateTimeFormatInfo DefaultDateTimeFormat = new DateTimeFormatInfo
        {
            FullDateTimePattern = "yyyy/MM/dd"
        };

        [Theory]
        [InlineData("2019/09/09", 2)]
        [InlineData("2019/09/10", 2)]
        [InlineData("2019/09/12", 2)]
        [InlineData("2019/10/18", 1)]
        public void identifies_date_of_transaction_correctly_for_all_transactions(string dateText, int expectedOccurrences)
        {
            // +Arrange
            var expectedDate = DateTimeOffset.Parse(dateText, DefaultDateTimeFormat);

            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Count(x => x.TransactionDate == expectedDate).Should().Be(expectedOccurrences);
        }


        [Theory]
        [InlineData("2019091001")]
        [InlineData("2019091002")]
        [InlineData("2019091101")]
        [InlineData("2019091201")]
        [InlineData("2019091202")]
        [InlineData("2019091301")]
        [InlineData("2019101801")]
        public void identifies_unique_id_correctly_for_all_transactions(string uniqueId)
        {
            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Select(x => x.UniqueId).Should().Contain(uniqueId);
        }

        [Theory]
        [InlineData("DEBIT", 6)]
        [InlineData("CREDIT", 1)]
        public void identifies_transaction_type_correctly_for_all_transactions(string transactionType, int expectedCount)
        {
            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Count(x => x.TransactionType == transactionType).Should().Be(expectedCount);
        }

        [Fact]
        public void identifies_payee_correctly_for_all_transactions()
        {
            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Any(x => !string.IsNullOrEmpty(x.Payee)).Should().BeFalse();
        }

        [Theory]
        [InlineData("DAMN BANKS AND THEIR FEES")]
        [InlineData("DEBIT 0")]
        [InlineData("DEBIT 1")]
        [InlineData("DEBIT 2")]
        [InlineData("DEBIT 3")]
        [InlineData("DEBIT 4")]
        [InlineData("PAYMENT RECEIVED THANK YOU")]
        public void identifies_description_correctly_for_all_transactions(string description)
        {
            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Any(x => x.Description == description).Should().BeTrue();
        }

        [Theory]
        [InlineData(20.00)]
        [InlineData(4.80)]
        [InlineData(27.75)]
        [InlineData(14.80)]
        [InlineData(274.16)]
        [InlineData(9.80)]
        [InlineData(-25000.00)]
        public void identifies_amount_correctly_for_all_transactions(decimal amount)
        {
            // +Assert
            _fixture.Snapshot.Should().NotBeNull();
            _fixture.Snapshot.Transactions.Should().NotBeNullOrEmpty();
            _fixture.Snapshot.Transactions.Select(x => x.Amount).Should().Contain(amount);
        }
    }
}