using System;

namespace Contracts
{
    public class TransactionImportJob
    {
        public Guid Id { get; set; }
        public DateTimeOffset FromDate { get; set; }
        public DateTimeOffset ToDate { get; set; }
        public Guid AccountId { get; set; }
        public string? SourceFileName { get; set; }
        public int TransactionCount { get; set; }
        public string Status { get; set; } = "Unknown";
    }
}
