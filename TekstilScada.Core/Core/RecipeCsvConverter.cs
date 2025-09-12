// Core/RecipeCsvConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TekstilScada.Models;

namespace TekstilScada.Core
{
    public static class RecipeCsvConverter
    {
        /// <summary>
        /// Bir ScadaRecipe nesnesini HMI'ın anlayacağı CSV formatına dönüştürür.
        /// </summary>
        public static string ToCsv(ScadaRecipe recipe)
        {
            if (recipe == null || recipe.Steps == null) return "";

            var csvBuilder = new StringBuilder();

            // Başlık bölümü
            string formattedDate = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            csvBuilder.AppendLine($"Date\t{formattedDate}");
            csvBuilder.AppendLine("Title\t");
            int itemCount = recipe.Steps.Count * 25;
            csvBuilder.AppendLine($"Item Count\t{itemCount}");

            // Veri bölümü
            foreach (var step in recipe.Steps.OrderBy(s => s.StepNumber))
            {
                foreach (var word in step.StepDataWords)
                {
                    csvBuilder.AppendLine(word.ToString());
                }
            }

            // *** EKLENEN DÜZELTME ***
            // HMI'ın dosyayı doğru işlemesi için gereken son boş satırı ekliyoruz.
            csvBuilder.AppendLine();

            return csvBuilder.ToString();
        }

        /// <summary>
        /// Bir CSV dosyasının içeriğini ScadaRecipe nesnesine dönüştürür.
        /// </summary>
        public static ScadaRecipe ToRecipe(string csvContent, string recipeName)
        {
            var recipe = new ScadaRecipe { RecipeName = recipeName, Steps = new List<ScadaRecipeStep>() };
            if (string.IsNullOrWhiteSpace(csvContent)) return recipe;

            // Unicode BOM'u varsa kaldır.
            string cleanedContent = csvContent;
            if (cleanedContent.Length > 0 && cleanedContent[0] == '\uFEFF')
            {
                cleanedContent = cleanedContent.Substring(1);
            }

            var lines = cleanedContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            int dataStartIndex = 0;
            if (lines.Count > 3 && lines[0].StartsWith("Date") && lines[2].StartsWith("Item Count"))
            {
                dataStartIndex = 3;
            }

            var numericLines = new List<string>();
            foreach (var line in lines.Skip(dataStartIndex))
            {
                var trimmedLine = line.Trim().Replace("\0", string.Empty);
                if (short.TryParse(trimmedLine, out _) || ushort.TryParse(trimmedLine, out _))
                {
                    numericLines.Add(trimmedLine);
                }
            }

            int stepNumber = 1;
            for (int i = 0; i < numericLines.Count; i += 25)
            {
                var stepValues = numericLines.Skip(i).Take(25).ToList();
                if (stepValues.Count == 25)
                {
                    try
                    {
                        var step = new ScadaRecipeStep { StepNumber = (short)stepNumber++ };
                        for (int j = 0; j < 25; j++)
                        {
                            if (short.TryParse(stepValues[j], out short signedValue))
                            {
                                step.StepDataWords[j] = signedValue;
                            }
                            else if (ushort.TryParse(stepValues[j], out ushort unsignedValue))
                            {
                                step.StepDataWords[j] = unchecked((short)unsignedValue);
                            }
                            else
                            {
                                // Handle cases where parsing fails entirely, e.g., set to 0 or log an error.
                                step.StepDataWords[j] = 0;
                            }
                        }
                        recipe.Steps.Add(step);
                    }
                    catch
                    {
                        // Hatalı satırları yoksay
                    }
                }
            }
            return recipe;
        }
    }
}