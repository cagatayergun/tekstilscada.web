// YENİ DOSYA: TekstilScada.Core/Models/Alarm.cs

using System;

namespace TekstilScada.Core.Models
{
    public class Alarm
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public int AlarmDefinitionId { get; set; }
        public string BatchId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Bu özellikler veritabanından JOIN ile doldurulur, 'alarms' tablosunda doğrudan yer almaz.
        public string AlarmCode { get; set; }
        public string AlarmMessage { get; set; }
        public string Severity { get; set; }

        // Bu özellik, kod içinde hesaplama ile doldurulur.
        public double DurationSeconds
        {
            get
            {
                // Bitiş zamanı başlangıçtan küçük olamaz, olursa 0 döner.
                if (EndTime < StartTime) return 0;
                return (EndTime - StartTime).TotalSeconds;
            }
        }
    }
}