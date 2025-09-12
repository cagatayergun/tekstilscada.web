// Models/AlarmHistory.cs
using System;

namespace TekstilScada.Models
{
    public class AlarmHistory
    {
        public long Id { get; set; }
        public int AlarmDefinitionId { get; set; }
        public int MachineId { get; set; }
        public string EventType { get; set; } // ACTIVE, INACTIVE, ACKNOWLEDGED
        public DateTime EventTimestamp { get; set; }
        public int? AcknowledgedByUserId { get; set; }
    }
}