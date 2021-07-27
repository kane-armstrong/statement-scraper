using System;

namespace Contracts
{
    public class TransactionImportJob
    {
        public int Id { get; set; }
        public DateTimeOffset FromDate { get; set; }
        public DateTimeOffset ToDate { get; set; }
        public int AccountId { get; set; }
        public string? SourceFileName { get; set; }
        public int TransactionCount { get; set; }
        public string Status { get; set; } = "Unknown";
    }
}
