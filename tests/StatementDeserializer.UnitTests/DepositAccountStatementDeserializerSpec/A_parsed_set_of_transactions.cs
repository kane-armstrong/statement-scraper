using FluentAssertions;
using StatementDeserializer.UnitTests.Resources.Content.DepositAccounts;
using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace StatementDeserializer.UnitTests.DepositAccountStatementDeserializerSpec;

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
        const int expectedNumberOfTransactions = 5;

        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Count.Should().Be(expectedNumberOfTransactions);
    }

    [Fact]
    public void does_not_contain_any_silent_parsing_failures()
    {
        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Should().NotContainNulls();
    }

    private static readonly DateTimeFormatInfo DefaultDateTimeFormat = new DateTimeFormatInfo
    {
        FullDateTimePattern = "yyyy/MM/dd"
    };

    [Theory]
    [InlineData("2012/01/02")]
    [InlineData("2018/12/27")]
    [InlineData("2018/12/28")]
    [InlineData("2018/12/31")]
    [InlineData("2019/01/02")]
    public void identifies_date_correctly_for_all_transactions(string dateText)
    {
        // +Arrange
        var expectedDate = DateTimeOffset.Parse(dateText, DefaultDateTimeFormat);

        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Select(x => x.TransactionDate).Should().Contain(expectedDate);
    }

    [Theory]
    [InlineData("2012010201")]
    [InlineData("2018122701")]
    [InlineData("2018122801")]
    [InlineData("2018123101")]
    [InlineData("2019010201")]
    public void identifies_unique_id_correctly_for_all_transactions(string uniqueId)
    {
        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Select(x => x.UniqueId).Should().Contain(uniqueId);
    }

    [Theory]
    [InlineData("BANK FEE", 2)]
    [InlineData("DEBIT", 2)]
    [InlineData("D/D", 1)]
    public void identifies_transaction_type_correctly_for_all_transactions(string transactionType, int expectedCount)
    {
        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Count(x => x.TransactionType == transactionType).Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("DAMN BANKS AND THEIR FEES", 2)]
    [InlineData("DEBIT", 2)]
    [InlineData("SomeInsurer", 1)]
    public void identifies_payee_correctly_for_all_transactions(string payee, int expectedCount)
    {
        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Count(x => x.Payee == payee).Should().Be(expectedCount);
    }

    [Theory]
    [InlineData("", 2)]
    [InlineData("OffshoreServiceMargins**", 1)]
    [InlineData("D/D Insurer ABC123456", 1)]
    [InlineData("USD 10.99 Udemy  at 0.6714*", 1)]
    public void identifies_memo_correctly_for_all_transactions(string memo, int expectedCount)
    {
        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Count(x => x.Description == memo).Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(-2.40)]
    [InlineData(-16.37)]
    [InlineData(-0.34)]
    [InlineData(-105.00)]
    [InlineData(-2.00)]
    public void identifies_amount_correctly_for_all_transactions(decimal amount)
    {
        // +Assert
        _fixture.Statement.Should().NotBeNull();
        _fixture.Statement.Transactions.Should().NotBeNullOrEmpty();
        _fixture.Statement.Transactions.Select(x => x.Amount).Should().Contain(amount);
    }

    [Fact]
    public void throws_format_exception_when_date_is_not_in_expected_format()
    {
        // +Arrange
        var sut = new DepositAccountStatementDeserializer();

        // +Act
        var exception = Assert.Throws<FormatException>(() => sut.DeserializeTdv(SampleDepositAccountReports.TdvTransactionMalformedDate));

        // +Assert
        exception.Message.Should().Be("Expected a date value in the format 'yyyy/MM/dd' in the date index (0); given 'oops'.");
    }

    [Fact]
    public void throws_format_exception_when_amount_is_not_decimal()
    {
        // +Arrange
        var sut = new DepositAccountStatementDeserializer();

        // +Act
        var exception = Assert.Throws<FormatException>(() => sut.DeserializeTdv(SampleDepositAccountReports.TdvTransactionMalformedAmount));

        // +Assert
        exception.Message.Should().Be("Expected a currency value in the amount index (6); given 'oops'.");
    }
}