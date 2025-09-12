namespace TekstilScada.Models
{
    /// <summary>
    /// OEE Raporu için hesaplanmış verileri tutar.
    /// </summary>
    public class OeeData
    {
        public string MachineName { get; set; }
        public string BatchId { get; set; }
        public double Availability { get; set; } // Kullanılabilirlik (%)
        public double Performance { get; set; }  // Performans (%)
        public double Quality { get; set; }      // Kalite (%)
        public double OEE { get; set; }          // Toplam Ekipman Etkinliği (%)
    }
}