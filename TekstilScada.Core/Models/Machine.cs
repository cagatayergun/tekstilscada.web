// Models/Machine.cs
namespace TekstilScada.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string MachineUserDefinedId { get; set; }
        public string MachineName { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string MachineType { get; set; }
        public bool IsEnabled { get; set; }
        public string VncAddress { get; set; }
        public string VncPassword { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string MachineSubType { get; set; } // YENİ

        public string DisplayInfo => $"{MachineName} - ({MachineUserDefinedId})";
    }
}