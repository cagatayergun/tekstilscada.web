// Models/AlarmDefinition.cs
namespace TekstilScada.Models
{
    public class AlarmDefinition
    {
        public int Id { get; set; }
        public int AlarmNumber { get; set; }
        public string AlarmText { get; set; }
        public int Severity { get; set; } // 1: Düşük, 4: Kritik
        public string Category { get; set; }
    }
}