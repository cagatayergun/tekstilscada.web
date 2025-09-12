//# ===== YENİ DOSYA: Core/RampCalculator.cs =====
using System;
using System.Collections.Generic;
using TekstilScada.Models;

namespace TekstilScada.Core
{
    /// <summary>
    /// Gradyan (Rampa) raporlaması için teorik sıcaklık grafiğini hesaplar.
    /// </summary>
    public static class RampCalculator
    {
        public static (double[] timestamps, double[] temperatures) GenerateTheoreticalRamp(ScadaRecipe recipe, DateTime startTime)
        {
            var timestamps = new List<double>();
            var temperatures = new List<double>();

            if (recipe == null || recipe.Steps == null)
                return (timestamps.ToArray(), temperatures.ToArray());

            double lastTemp = 25; // Başlangıç sıcaklığı varsayımı
            DateTime currentTime = startTime;

            timestamps.Add(currentTime.ToOADate());
            temperatures.Add(lastTemp);

            foreach (var step in recipe.Steps)
            {
                // Adım tipi "Isıtma" ise (Word 24, Bit 1)
                if ((step.StepDataWords[24] & 2) != 0)
                {
                    double targetTemp = step.StepDataWords[3] / 10.0; // Isı (°C)
                    double durationMinutes = step.StepDataWords[4]; // Süre (DK)

                    // Eğer ısıtma adımıysa, rampa hesaplanır.
                    if (durationMinutes > 0 && targetTemp > lastTemp)
                    {
                        currentTime = currentTime.AddMinutes(durationMinutes);
                        timestamps.Add(currentTime.ToOADate());
                        temperatures.Add(targetTemp);
                        lastTemp = targetTemp;
                    }
                }
            }

            return (timestamps.ToArray(), temperatures.ToArray());
        }
    }
}
