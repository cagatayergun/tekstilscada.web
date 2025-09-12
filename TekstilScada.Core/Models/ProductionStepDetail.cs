// Models/ProductionStepDetail.cs
using System;

namespace TekstilScada.Models
{
    // Bu sınıf, eski programınızdaki 'analız' tablosunun karşılığıdır.
    public class ProductionStepDetail
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string TheoreticalTime { get; set; }
        public string WorkingTime { get; set; }
        public string StopTime { get; set; }
        public string DeflectionTime { get; set; }
    }
}