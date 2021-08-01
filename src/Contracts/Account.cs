using System;

namespace Contracts
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public string? CardOrAccountNumber { get; set; }
    }
}
