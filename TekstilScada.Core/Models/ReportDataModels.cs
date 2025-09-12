// Models/ReportDataModels.cs

namespace TekstilScada.Models
{
    /// <summary>
    /// Üretim sonu toplam tüketim verilerini tutar.
    /// </summary>
    public class BatchSummaryData
    {
        public short TotalWater { get; set; }
        public short TotalElectricity { get; set; }
        public short TotalSteam { get; set; }
    }

    /// <summary>
    /// Üretim sonu gerçekleşen tek bir kimyasal tüketim verisini tutar.
    /// </summary>
    public class ChemicalConsumptionData
    {
        public int StepNumber { get; set; }
        public string ChemicalName { get; set; }
        public short AmountLiters { get; set; }
    }
}
