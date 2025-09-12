// Models/ScadaRecipe.cs
using System.Collections.Generic;
using System;

namespace TekstilScada.Models
{
    public class ScadaRecipe
    {
        public int Id { get; set; }
        public string RecipeName { get; set; }
        public string TargetMachineType { get; set; } // YENİ EKLENEN SATIR
        public DateTime CreationDate { get; set; }
        public List<ScadaRecipeStep> Steps { get; set; }

        public ScadaRecipe()
        {
            Steps = new List<ScadaRecipeStep>();
        }
    }
}