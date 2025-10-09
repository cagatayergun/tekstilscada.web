// Dosya: TekstilScada.WebAPI/Controllers/ReportsController.cs
using Microsoft.AspNetCore.Mvc;
using TekstilScada.Repositories;
using System;
using System.Text.Json.Serialization;
using System.Globalization; // CultureInfo için eklendi

namespace TekstilScada.WebAPI.Controllers
{
    // --- YENİ DTO: Tarihleri string olarak alıyoruz ---
    public class ReportFiltersDto
    {
        [JsonPropertyName("startTime")]
        public string StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public string EndTime { get; set; }

        [JsonPropertyName("machineId")]
        public int? MachineId { get; set; }

        [JsonPropertyName("batchNo")]
        public string BatchNo { get; set; }

        [JsonPropertyName("recipeName")]
        public string RecipeName { get; set; }

        [JsonPropertyName("siparisNo")]
        public string SiparisNo { get; set; }

        [JsonPropertyName("musteriNo")]
        public string MusteriNo { get; set; }

        [JsonPropertyName("operatorName")]
        public string OperatorName { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ProductionRepository _productionRepository;

        public ReportsController(ProductionRepository productionRepository)
        {
            _productionRepository = productionRepository;
        }

        [HttpPost("production")]
        public IActionResult GetProductionReport([FromBody] ReportFiltersDto filtersDto)
        {
            try
            {
                // --- ÇÖZÜM: Gelen string tarihleri DateTime'a manuel çeviriyoruz ---
                var coreFilters = new ReportFilters
                {
                    // "O" formatı, gelen ISO 8601 formatındaki (saat dilimi dahil) tarihi doğru şekilde parse eder.
                    StartTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    EndTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    MachineId = filtersDto.MachineId,
                    BatchNo = filtersDto.BatchNo,
                    RecipeName = filtersDto.RecipeName,
                    SiparisNo = filtersDto.SiparisNo,
                    MusteriNo = filtersDto.MusteriNo,
                    OperatorName = filtersDto.OperatorName
                };

                var reportData = _productionRepository.GetProductionReport(coreFilters);
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Rapor oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }
    }
}