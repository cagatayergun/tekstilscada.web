// Services/PlcManagerFactory.cs
using System;
using TekstilScada.Models;

namespace TekstilScada.Services
{
    /// <summary>
    /// Verilen makine tipine göre uygun IPlcManager nesnesini oluşturan fabrika sınıfı.
    /// </summary>
    public static class PlcManagerFactory
    {
        public static IPlcManager Create(Machine machine)
        {
            switch (machine.MachineType)
            {
                case "BYMakinesi":
                    return new BYMakinesiManager(machine.IpAddress, machine.Port);

                case "Kurutma Makinesi":
                    return new KurutmaMakinesiManager(machine.IpAddress, machine.Port);

                default:
                    // Eğer bilinmeyen bir makine tipi gelirse, programın çökmemesi için bir istisna fırlat.
                    throw new ArgumentException($"Bilinmeyen makine tipi: '{machine.MachineType}'. Lütfen makine ayarlarını kontrol edin.");
            }
        }
    }
}
