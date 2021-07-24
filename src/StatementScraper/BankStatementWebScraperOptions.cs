namespace StatementScraper
{
    public class BankStatementWebScraperOptions
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = string.Empty;
        public string DownloadPath { get; set; } = string.Empty;
    }
}