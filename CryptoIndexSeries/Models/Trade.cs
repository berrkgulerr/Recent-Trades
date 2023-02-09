namespace CryptoIndexSeries.Models
{
    public class Trade
    {
        public string? AssetSymbol { get; set; }
        public string? AssetQuantity { get; set; }
        public string? QuoteAssetSymbol { get; set; }
        public string? QuoteAssetQuantity { get; set; }
        public string? Date { get; set; }
    }

}
