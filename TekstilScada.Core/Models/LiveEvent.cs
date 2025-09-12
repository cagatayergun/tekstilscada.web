
    using System;

namespace TekstilScada.Models
    {
        public enum EventType { SystemInfo, SystemWarning, SystemSuccess, Alarm, ConnectionStateChange, Process }

        public class LiveEvent
        {
            public DateTime Timestamp { get; set; }
            public EventType Type { get; set; }
            public string Source { get; set; }
            public string Message { get; set; }

            public LiveEvent() { Timestamp = DateTime.Now; }
        }
    
}