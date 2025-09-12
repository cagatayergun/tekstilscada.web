using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
//using System.Windows.Forms;
using TekstilScada.Core;
using TekstilScada.Models;
using TekstilScada.Repositories;

//#region Adım 1: Yeni Model ve Yardımcı Sınıflar (TekstilScada.Core Projesi)

// --- YENİ DOSYA: TekstilScada.Core/Models/ControlMetadata.cs ---
namespace TekstilScada.Models
{
    // JSON'da saklanacak kontrol bilgilerini temsil eden sınıf
    public class ControlMetadata
    {
        public string ControlType { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Location { get; set; } // "X, Y" formatında
        public string Size { get; set; }     // "Width, Height" formatında
        public decimal Maximum { get; set; } = 1000; // NumericUpDown için
          // YENİ EKLENEN ALAN
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int DecimalPlaces { get; set; }

                                                     // YENİ: TextBox için string uzunluğunu Word cinsinden belirtir.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int StringWordLength { get; set; }
        // PLC Eşleme Bilgileri
        [JsonPropertyName("PLC_WordIndex")]
        public int PLC_WordIndex { get; set; }

        [JsonPropertyName("PLC_BitIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Değer 0 ise JSON'a yazma
        public int PLC_BitIndex { get; set; }
    }

    // Çalışma zamanında kontrolün Tag özelliğinde saklanacak PLC adresi bilgisi
    public class PlcMapping
    {
        public int WordIndex { get; set; }
        public int BitIndex { get; set; }
        public int StringWordLength { get; set; } // YENİ
    }
}