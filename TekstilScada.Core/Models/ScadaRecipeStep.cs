// Models/ScadaRecipeStep.cs
namespace TekstilScada.Models
{
    public class ScadaRecipeStep
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int StepNumber { get; set; }

        // Adım verilerini ham olarak tutacağız.
        // Her word, bir dizi elemanına karşılık gelir.
        public short[] StepDataWords { get; set; }

        public ScadaRecipeStep()
        {
            // Her adım 25 word'den oluşur.
            StepDataWords = new short[25];
        }
    }
}