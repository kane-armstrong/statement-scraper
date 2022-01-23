using Contracts;
using StatementDeserializer.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StatementDeserializer;

public class DepositAccountStatementDeserializer : IStatementDeserializer
{
    public Statement? DeserializeTdv(byte[] fileBytes)
    {
        var content = Encoding.UTF8.GetString(fileBytes);
        var lines = content.Split(Environment.NewLine);

        // Make sure its a deposit account statement
        if (!lines[AccountLineIndex].Contains("Bank"))
            return null;

        var account = ParseAccount(lines[AccountLineIndex]);

        var transactions = ParseTransactions(lines);

        return new Statement
        {
            CardOrAccountNumber = account,
            Transactions = transactions
        };
    }

    public AccountType AccountType => AccountType.DepositAccount;

    private const int AccountLineIndex = 1;

    private static List<Transaction> ParseTransactions(IEnumerable<string> lines)
    {
        const int linesBeforeTransactionsStart = 7;

        var relevantLines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).Skip(linesBeforeTransactionsStart);

        return relevantLines.Select(ParseTransaction).ToList();
    }

    private const string ReportDateTimeFormat = "yyyy/MM/dd";

    private static readonly DateTimeFormatInfo ReportFormatInfo = new DateTimeFormatInfo
    {
        FullDateTimePattern = ReportDateTimeFormat
    };

    private static Transaction ParseTransaction(string line)
    {
        const char delimiter = '\t';

        const int dateIndex = 0;
        const int uniqueIdIndex = 1;
        const int transactionTypeIndex = 2;
        const int payeeIndex = 4;
        const int descriptionIndex = 5;
        const int amountIndex = 6;

        var parts = line.Split(delimiter);

        var dateText = parts[dateIndex];
        var uniqueId = parts[uniqueIdIndex];
        var transactionType = parts[transactionTypeIndex];
        var payee = parts[payeeIndex].RemoveQuotationMarks();
        var description = parts[descriptionIndex].RemoveQuotationMarks();
        var amountText = parts[amountIndex];

        var dateParseable = DateTimeOffset.TryParse(dateText, ReportFormatInfo, DateTimeStyles.AssumeLocal, out var date);
        if (!dateParseable)
        {
            throw new FormatException($"Expected a date value in the format '{ReportDateTimeFormat}' in the date index ({dateIndex}); given '{dateText}'.");
        }

        var amountParseable = decimal.TryParse(amountText, out var amount);
        if (!amountParseable)
        {
            throw new FormatException($"Expected a currency value in the amount index ({amountIndex}); given '{amountText}'.");
        }

        return new Transaction
        {
            TransactionDate = date,
            UniqueId = uniqueId,
            TransactionType = transactionType,
            Payee = payee,
            Description = description,
            Amount = amount
        };
    }

    private static string ParseAccount(string line)
    {
        const char delimiter = ';';

        var parts = line.Split(delimiter);

        var bankText = parts[0].Trim().Split(' ')[1];
        var branchText = parts[1].Trim().Split(' ')[1];

        var accountParts = parts[2].Trim().Split(' ');
        var number = accountParts[1];

        var name = accountParts.Length > 2
            ? accountParts[2].Replace("(", "").RemoveParentheses()
            : "Unknown";

        var bankParsed = int.TryParse(bankText, out var bank);
        if (!bankParsed)
        {
            throw new FormatException($"Expected a numeric bank identifier; given '{bankText}'.");
        }

        var branchParsed = int.TryParse(branchText, out var branch);
        if (!branchParsed)
        {
            throw new FormatException($"Expected a numeric branch identifier; given '{branchText}'.");
        }

        return $"{bank}-{branch}-{number}-{name}";
    }
}