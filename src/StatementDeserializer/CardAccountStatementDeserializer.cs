using Contracts;
using StatementDeserializer.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StatementDeserializer
{
    public class CardAccountStatementDeserializer : IStatementDeserializer
    {
        public TransactionsSnapshot? DeserializeTdv(byte[] fileBytes)
        {
            var content = Encoding.UTF8.GetString(fileBytes);
            var lines = content.Split(Environment.NewLine);

            // Make sure its a card account statement
            if (!lines[CardLineIndex].Contains("Card Number"))
                return null;

            var card = ParseCard(lines[CardLineIndex]);

            var transactions = ParseTransactions(lines);

            return new TransactionsSnapshot
            {
                CardOrAccountNumber = card,
                Transactions = transactions
            };
        }

        public AccountType AccountType => AccountType.CardAccount;

        private const int CardLineIndex = 1;

        private static List<Transaction> ParseTransactions(IEnumerable<string> lines)
        {
            const int linesBeforeTransactionsStart = 5;

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

            const int dateIndex = 1;
            const int uniqueIdIndex = 2;
            const int transactionTypeIndex = 3;
            const int cardNumberIndex = 4;
            const int descriptionIndex = 5;
            const int amountIndex = 6;

            var parts = line.Split(delimiter);

            var dateText = parts[dateIndex];
            var uniqueId = parts[uniqueIdIndex];
            var transactionType = parts[transactionTypeIndex];
            var cardNumber = parts[cardNumberIndex];
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
                CardNumber = cardNumber,
                Payee = null,
                Description = description,
                Amount = amount
            };
        }

        private static string ParseCard(string line)
        {
            const char delimiter = ' ';
            return line.Split(delimiter)[2];
        }
    }
}
