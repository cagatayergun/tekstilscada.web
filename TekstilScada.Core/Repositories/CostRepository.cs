using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq; // LINQ kullanabilmek için eklendi
using TekstilScada.Models;
using System; // DBNull için eklendi
using TekstilScada.Core; // Bu satırı ekleyin

namespace TekstilScada.Repositories
{
    public class CostRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        public List<CostParameter> GetAllParameters()
        {
            var parameters = new List<CostParameter>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, ParameterName, CostValue, Unit, Multiplier, CurrencySymbol FROM cost_parameters;";
                var cmd = new MySqlCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    string currencySymbol = "TL"; // Varsayılan
                    // Önce tüm veriyi bir listeye alalım
                    var rawData = new List<dynamic>();
                    while (reader.Read())
                    {
                        rawData.Add(new
                        {
                            Id = reader.GetInt32("Id"),
                            ParameterName = reader.GetString("ParameterName"),
                            CostValue = reader.GetDecimal("CostValue"),
                            Unit = reader.GetString("Unit"),
                            Multiplier = reader.GetDecimal("Multiplier"),
                            Symbol = reader.IsDBNull(reader.GetOrdinal("CurrencySymbol")) ? null : reader.GetString("CurrencySymbol")
                        });
                    }

                    // Para birimi sembolünü null olmayan ilk kayıttan al
                    currencySymbol = rawData.FirstOrDefault(d => d.Symbol != null)?.Symbol ?? currencySymbol;

                    // Nihai listeyi oluştur
                    foreach (var d in rawData)
                    {
                        parameters.Add(new CostParameter
                        {
                            Id = d.Id,
                            ParameterName = d.ParameterName,
                            CostValue = d.CostValue,
                            Unit = d.Unit,
                            Multiplier = d.Multiplier,
                            CurrencySymbol = currencySymbol
                        });
                    }
                }
            }
            return parameters;
        }

        // HATA GİDERİLDİ: Metot tamamen yeniden ve daha güvenli bir şekilde yazıldı.
        public void UpdateParameters(List<CostParameter> updatedParameters)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // İlk parametreden para birimi sembolünü al
                        string currencySymbol = updatedParameters.FirstOrDefault()?.CurrencySymbol ?? "TL";

                        foreach (var param in updatedParameters)
                        {
                            string query = "UPDATE cost_parameters SET CostValue = @CostValue, Multiplier = @Multiplier, CurrencySymbol = @CurrencySymbol WHERE Id = @Id;";
                            var cmd = new MySqlCommand(query, connection, transaction);
                            cmd.Parameters.AddWithValue("@CostValue", param.CostValue);
                            cmd.Parameters.AddWithValue("@Multiplier", param.Multiplier);

                            // Para birimini sadece Id=1 olan kayda yaz, diğerlerini null yap. Bu daha güvenli bir yöntem.
                            if (param.Id == 1)
                            {
                                cmd.Parameters.AddWithValue("@CurrencySymbol", currencySymbol);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@CurrencySymbol", (object)DBNull.Value);
                            }

                            cmd.Parameters.AddWithValue("@Id", param.Id);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}