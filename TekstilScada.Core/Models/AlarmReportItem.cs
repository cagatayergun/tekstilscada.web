// Models/AlarmReportItem.cs
using System;

namespace TekstilScada.Models
{
    public class AlarmReportItem
    {
        public string MachineName { get; set; }
        public int AlarmNumber { get; set; }
        public string AlarmText { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Duration { get; set; } // "hh:mm:ss" formatında
    }
}
