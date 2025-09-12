namespace TekstilScada.Models
{
    public class CostParameter
    {
        public int Id { get; set; }
        public string ParameterName { get; set; }
        public decimal CostValue { get; set; }
        public string Unit { get; set; }
        public decimal Multiplier { get; set; } // YENİ
        public string CurrencySymbol { get; set; } // YENİ
    }
}