// Repositories/RecipeRepository.cs
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TekstilScada.Models;
using TekstilScada.Core; // Bu satırı ekleyin
namespace TekstilScada.Repositories
{
    public class RecipeRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        public void SaveRecipe(ScadaRecipe recipe)
        {
            // Bu metot daha önce oluşturuldu, Id kontrolü eklenerek güncellendi.
            if (recipe.Id > 0)
            {
                UpdateRecipe(recipe);
            }
            else
            {
                AddRecipe(recipe);
            }
        }

        private void AddRecipe(ScadaRecipe recipe)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string recipeQuery = "INSERT INTO recipes (RecipeName, TargetMachineType,CreationDate) VALUES (@RecipeName, @TargetMachineType, @CreationDate); SELECT LAST_INSERT_ID();";
                        var recipeCmd = new MySqlCommand(recipeQuery, connection, transaction);
                        recipeCmd.Parameters.AddWithValue("@RecipeName", recipe.RecipeName);
                        recipeCmd.Parameters.AddWithValue("@TargetMachineType", recipe.TargetMachineType); // Yeni parametreyi ekleyin
                        recipeCmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                        recipe.Id = Convert.ToInt32(recipeCmd.ExecuteScalar());

                        foreach (var step in recipe.Steps)
                        {
                            string stepQuery = "INSERT INTO recipe_steps (RecipeId, StepNumber, Word0, Word1, Word2, Word3, Word4, Word5, Word6, Word7, Word8, Word9, Word10, Word11, Word12, Word13, Word14, Word15, Word16, Word17, Word18, Word19, Word20, Word24) " +
                                               "VALUES (@RecipeId, @StepNumber, @Word0, @Word1, @Word2, @Word3, @Word4, @Word5, @Word6, @Word7, @Word8, @Word9, @Word10, @Word11, @Word12, @Word13, @Word14, @Word15, @Word16, @Word17, @Word18, @Word19, @Word20, @Word24);";

                            var stepCmd = new MySqlCommand(stepQuery, connection, transaction);
                            stepCmd.Parameters.AddWithValue("@RecipeId", recipe.Id);
                            stepCmd.Parameters.AddWithValue("@StepNumber", step.StepNumber);
                            for (int i = 0; i <= 24; i++)
                            {
                                if (i >= 21 && i <= 23) continue; // String alanları atla
                                stepCmd.Parameters.AddWithValue($"@Word{i}", step.StepDataWords[i]);
                            }
                            stepCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void UpdateRecipe(ScadaRecipe recipe)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Ana reçete adını güncelle
                        string recipeQuery = "UPDATE recipes SET RecipeName = @RecipeName, TargetMachineType = @TargetMachineType WHERE Id = @Id;";
                        var recipeCmd = new MySqlCommand(recipeQuery, connection, transaction);
                        recipeCmd.Parameters.AddWithValue("@RecipeName", recipe.RecipeName);
                        recipeCmd.Parameters.AddWithValue("@TargetMachineType", recipe.TargetMachineType); // Yeni parametreyi ekleyin
                        recipeCmd.Parameters.AddWithValue("@Id", recipe.Id);
                        recipeCmd.ExecuteNonQuery();

                        // 2. Bu reçeteye ait eski adımları sil
                        string deleteStepsQuery = "DELETE FROM recipe_steps WHERE RecipeId = @RecipeId;";
                        var deleteCmd = new MySqlCommand(deleteStepsQuery, connection, transaction);
                        deleteCmd.Parameters.AddWithValue("@RecipeId", recipe.Id);
                        deleteCmd.ExecuteNonQuery();

                        // 3. Yeni/güncellenmiş adımları ekle
                        foreach (var step in recipe.Steps)
                        {
                            // Adım ekleme sorgusu (AddRecipe ile aynı)
                            string stepQuery = "INSERT INTO recipe_steps (RecipeId, StepNumber, Word0, Word1, Word2, Word3, Word4, Word5, Word6, Word7, Word8, Word9, Word10, Word11, Word12, Word13, Word14, Word15, Word16, Word17, Word18, Word19, Word20, Word24) " +
                                              "VALUES (@RecipeId, @StepNumber, @Word0, @Word1, @Word2, @Word3, @Word4, @Word5, @Word6, @Word7, @Word8, @Word9, @Word10, @Word11, @Word12, @Word13, @Word14, @Word15, @Word16, @Word17, @Word18, @Word19, @Word20, @Word24);";
                            var stepCmd = new MySqlCommand(stepQuery, connection, transaction);
                            stepCmd.Parameters.AddWithValue("@RecipeId", recipe.Id);
                            stepCmd.Parameters.AddWithValue("@StepNumber", step.StepNumber);
                            for (int i = 0; i <= 24; i++)
                            {
                                if (i >= 21 && i <= 23) continue;
                                stepCmd.Parameters.AddWithValue($"@Word{i}", step.StepDataWords[i]);
                            }
                            stepCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception) { transaction.Rollback(); throw; }
                }
            }
        }
        public void AddOrUpdateRecipe(ScadaRecipe recipe)
        {
            // Bu metot artık doğrudan çağrılacak. İsim kontrolünü kendisi yapacak.
            var existingRecipe = GetAllRecipes().FirstOrDefault(r => r.RecipeName == recipe.RecipeName && r.TargetMachineType == recipe.TargetMachineType);

            if (existingRecipe != null)
            {
                recipe.Id = existingRecipe.Id;
                UpdateRecipe(recipe); // Mevcut UpdateRecipe metodunuzu çağırır
            }
            else
            {
                AddRecipe(recipe); // Mevcut AddRecipe metodunuzu çağırır
            }
        }
        // YENİ: Hata veren eksik metot eklendi.
        public List<ScadaRecipe> GetAllRecipes()
        {
            var recipes = new List<ScadaRecipe>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, RecipeName,TargetMachineType, CreationDate FROM recipes ORDER BY RecipeName;";
                var cmd = new MySqlCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recipes.Add(new ScadaRecipe
                        {
                            Id = reader.GetInt32("Id"),
                            RecipeName = reader.GetString("RecipeName"),
                            TargetMachineType = reader.IsDBNull(reader.GetOrdinal("TargetMachineType")) ? "Bilinmiyor" : reader.GetString("TargetMachineType"),
                            CreationDate = reader.GetDateTime("CreationDate")
                        });
                    }
                }
            }
            return recipes;
        }
        public void DeleteRecipe(int recipeId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // ON DELETE CASCADE sayesinde, sadece ana reçeteyi silmek yeterlidir.
                string query = "DELETE FROM recipes WHERE Id = @Id;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", recipeId);
                cmd.ExecuteNonQuery();
            }
        }
        // YENİ: Hata veren eksik metot eklendi.
        public ScadaRecipe GetRecipeById(int recipeId)
        {
            ScadaRecipe recipe = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string recipeQuery = "SELECT Id, RecipeName,TargetMachineType, CreationDate FROM recipes WHERE Id = @Id;";
                var recipeCmd = new MySqlCommand(recipeQuery, connection);
                recipeCmd.Parameters.AddWithValue("@Id", recipeId);

                using (var reader = recipeCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        recipe = new ScadaRecipe
                        {
                            Id = reader.GetInt32("Id"),
                            RecipeName = reader.GetString("RecipeName"),
                            TargetMachineType = reader.IsDBNull(reader.GetOrdinal("TargetMachineType")) ? "Bilinmiyor" : reader.GetString("TargetMachineType"),
                            CreationDate = reader.GetDateTime("CreationDate")
                        };
                    }
                }

                if (recipe != null)
                {
                    // Adımları yükle
                    string stepsQuery = "SELECT * FROM recipe_steps WHERE RecipeId = @RecipeId ORDER BY StepNumber;";
                    var stepsCmd = new MySqlCommand(stepsQuery, connection);
                    stepsCmd.Parameters.AddWithValue("@RecipeId", recipeId);

                    using (var reader = stepsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var step = new ScadaRecipeStep
                            {
                                Id = reader.GetInt32("Id"),
                                StepNumber = reader.GetInt32("StepNumber")
                            };
                            for (int i = 0; i <= 24; i++)
                            {
                                if (i >= 21 && i <= 23) continue;
                                step.StepDataWords[i] = reader.GetInt16($"Word{i}");
                            }
                            recipe.Steps.Add(step);
                        }
                    }
                }
            }
            return recipe;
        }
        public List<ProductionReportItem> GetRecipeUsageHistory(int recipeId)
        {
            var history = new List<ProductionReportItem>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                m.MachineName,
                b.BatchId,
                b.StartTime,
                b.EndTime,
                TIMEDIFF(b.EndTime, b.StartTime) as CycleTime,
                b.TotalWater,
                b.TotalElectricity,
                b.TotalSteam
            FROM production_batches AS b
            JOIN machines AS m ON b.MachineId = m.Id
            JOIN recipes AS r ON b.RecipeName = r.RecipeName
            WHERE r.Id = @RecipeId AND b.EndTime IS NOT NULL
            ORDER BY b.StartTime DESC;";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RecipeId", recipeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(new ProductionReportItem
                        {
                            MachineName = reader.GetString("MachineName"),
                            BatchId = reader.GetString("BatchId"),
                            StartTime = reader.GetDateTime("StartTime"),
                            EndTime = reader.GetDateTime("EndTime"),
                            CycleTime = reader.IsDBNull(reader.GetOrdinal("CycleTime"))
                                        ? "N/A"
                                        : reader.GetTimeSpan(reader.GetOrdinal("CycleTime")).ToString(@"hh\:mm\:ss"),

                            // GÜNCELLENDİ: Tüketim verileri artık okunuyor.
                            TotalWater = reader.IsDBNull(reader.GetOrdinal("TotalWater")) ? 0 : reader.GetInt32("TotalWater"),
                            TotalElectricity = reader.IsDBNull(reader.GetOrdinal("TotalElectricity")) ? 0 : reader.GetInt32("TotalElectricity"),
                            TotalSteam = reader.IsDBNull(reader.GetOrdinal("TotalSteam")) ? 0 : reader.GetInt32("TotalSteam")
                        });
                    }
                }
            }
            return history;
        }
        public ScadaRecipe GetRecipeByName(string recipeName)
        {
            int recipeId = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id FROM recipes WHERE RecipeName = @RecipeName LIMIT 1;";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RecipeName", recipeName);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    recipeId = Convert.ToInt32(result);
                }
            }
            if (recipeId > 0)
            {
                return GetRecipeById(recipeId);
            }
            return null;
        }
    }
}
