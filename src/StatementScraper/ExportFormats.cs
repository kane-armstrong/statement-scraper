using System.Collections.Generic;

namespace StatementScraper
{
    public static class ExportFormats
    {
        // Found by running PageExtensions.GetSelectListOptions on the select element for export format
        public const string OfxMsMoney = "OFX - MS Money";
        public const string OfxQuicken = "OFX - Quicken";
        public const string QifQuicken = "QIF - Quicken";
        public const string CsvGeneric = "CSV - Generic";
        public const string TdvGeneric = "TDV - Generic";
        
        internal static IEnumerable<string> List()
        {
            yield return OfxMsMoney;
            yield return OfxQuicken;
            yield return QifQuicken;
            yield return CsvGeneric;
            yield return TdvGeneric;
        }
    }
}