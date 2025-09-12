using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
//using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Repositories;



// --- YENİ DOSYA: TekstilScada.Core/Models/ControlMetadata.cs ---

namespace TekstilScada.Repositories
{
    // Veritabanındaki yeni step_editor_layouts tablosuyla iletişim kurar
    public class RecipeConfigurationRepository
    {
        private readonly string _connectionString = AppConfig.ConnectionString;

        public string GetLayoutJson(string machineSubType, int stepTypeId)
        {
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT LayoutJson FROM step_editor_layouts WHERE MachineSubType = @SubType AND StepTypeId = @StepId";
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SubType", machineSubType ?? "DEFAULT");
                cmd.Parameters.AddWithValue("@StepId", stepTypeId);
                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value) return result.ToString();

                // Makineye özel tanım yoksa, varsayılanı (DEFAULT) dene
                cmd.Parameters["@SubType"].Value = "DEFAULT";
                result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        // Tasarımcıdan gelen JSON'ı veritabanına kaydeder/günceller
        public void SaveLayout(string layoutName, string machineSubType, int stepTypeId, string layoutJson)
        {
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
            {
                connection.Open();
                // ON DUPLICATE KEY UPDATE sayesinde kayıt varsa günceller, yoksa ekler.
                // Bunun çalışması için veritabanında (MachineSubType, StepTypeId) üzerinde UNIQUE KEY olması gerekir.
                string query = @"
                    INSERT INTO step_editor_layouts (LayoutName, MachineSubType, StepTypeId, LayoutJson)
                    VALUES (@LayoutName, @MachineSubType, @StepTypeId, @LayoutJson)
                    ON DUPLICATE KEY UPDATE LayoutName = @LayoutName, LayoutJson = @LayoutJson;";
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@LayoutName", layoutName);
                cmd.Parameters.AddWithValue("@MachineSubType", machineSubType);
                cmd.Parameters.AddWithValue("@StepTypeId", stepTypeId);
                cmd.Parameters.AddWithValue("@LayoutJson", layoutJson);
                cmd.ExecuteNonQuery();
            }
        }
        public DataTable GetStepTypes()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT Id, StepName FROM step_types ORDER BY Id";
                var adapter = new MySqlDataAdapter(query, connection);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        // YENİ: Veritabanındaki tüm farklı makine alt tiplerini çeker.
        public List<string> GetMachineSubTypes()
        {
            var subTypes = new List<string> { "DEFAULT" }; // Varsayılan her zaman olmalı
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                // Sadece 'BYMakinesi' tipindeki makinelerin alt tiplerini al
                string query = "SELECT DISTINCT MachineSubType FROM machines WHERE MachineType = 'BYMakinesi' AND MachineSubType IS NOT NULL AND MachineSubType <> ''";
                var cmd = new MySqlCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subTypes.Add(reader.GetString(0));
                    }
                }
            }
            return subTypes;
        }
    }
}