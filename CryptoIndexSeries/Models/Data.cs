namespace CryptoIndexSeries.Models
{
    public class Data
    {
        public int? Id { get; set; }
        public string? Exchange { get; set; }
        public List<SymbolData>? SymbolData { get; set; }
    }

    public class SymbolData
    {
        public string? Symbol { get; set; }
        public string? Base { get; set; }
        public string? Quote { get; set; }
    }
}
