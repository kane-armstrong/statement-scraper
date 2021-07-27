using System;

namespace StatementScraper
{
    public class DownloadResult
    {
        public bool Successful { get; set; }
        public string StatusMessage { get; set; }

        internal DownloadResult(bool successful, string statusMessage)
        {
            Successful = successful;
            StatusMessage = statusMessage ?? throw new ArgumentNullException(nameof(statusMessage));
        }

        public static DownloadResult Success => new DownloadResult(true, "Successful");
        public static DownloadResult Failed(string reason) => new DownloadResult(false, reason);
    }
}
