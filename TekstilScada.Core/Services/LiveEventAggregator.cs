// TekstilScada.Core/Services/LiveEventAggregator.cs
using System;
using TekstilScada.Models;

namespace TekstilScada.Services
{
    // LiveEvent ve EventType enum'ları burada yer alıyor. Bunlarda değişiklik yok.
    public class LiveEvent
    {
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public EventType Type { get; set; }

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss} | {Source} | {Message}";
        }
    }

    public enum EventType
    {
        Alarm,
        Process,
        SystemInfo,
        SystemWarning,
        SystemSuccess
    }

    // Bu sınıfın tamamını güncelleyin
    public class LiveEventAggregator
    {
        // Singleton (Tek Nesne) oluşturma
        private static readonly LiveEventAggregator _instance = new LiveEventAggregator();

        // HATANIN OLDUĞU SATIRIN DÜZELTİLMİŞ HALİ: .Value kaldırıldı
        public static LiveEventAggregator Instance => _instance;

        public event Action<LiveEvent> OnEventPublished;

        private LiveEventAggregator() { }

        public void Publish(LiveEvent liveEvent)
        {
            OnEventPublished?.Invoke(liveEvent);
        }

        // --- YARDIMCI METOTLAR ---
        public void PublishAlarm(string machineName, string message)
        {
            Publish(new LiveEvent { Timestamp = DateTime.Now, Source = machineName, Message = message, Type = EventType.Alarm });
        }

        public void PublishSystemInfo(string source, string message)
        {
            Publish(new LiveEvent { Timestamp = DateTime.Now, Source = source, Message = message, Type = EventType.SystemInfo });
        }

        public void PublishSystemWarning(string source, string message)
        {
            Publish(new LiveEvent { Timestamp = DateTime.Now, Source = source, Message = message, Type = EventType.SystemWarning });
        }

        public void PublishSystemSuccess(string source, string message)
        {
            Publish(new LiveEvent { Timestamp = DateTime.Now, Source = source, Message = message, Type = EventType.SystemSuccess });
        }
    }
}