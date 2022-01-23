using System;

namespace Contracts;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string UniqueId { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string? Payee { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}