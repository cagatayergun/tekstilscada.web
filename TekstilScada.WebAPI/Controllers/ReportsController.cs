﻿// TekstilScada.WebAPI/Controllers/ReportsController.cs

using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json.Serialization;
using TekstilScada.Core.Models;
using TekstilScada.Models;
using TekstilScada.Repositories;

public class GeneralDetailedConsumptionFilters
{
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }

    // Çoklu seçim için List<int> MachineIds
    public List<int>? MachineIds { get; set; }
}
public class ActionLogFiltersDto
{
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
public class ReportFiltersDto
{
    // DÜZELTME: Tüm string alanları nullable (string?) yapıldı
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("machineId")]
    public int? MachineId { get; set; }

    [JsonPropertyName("batchNo")]
    public string? BatchNo { get; set; }

    [JsonPropertyName("recipeName")]
    public string? RecipeName { get; set; }

    [JsonPropertyName("siparisNo")]
    public string? SiparisNo { get; set; }

    [JsonPropertyName("musteriNo")]
    public string? MusteriNo { get; set; }

    [JsonPropertyName("operatorName")]
    public string? OperatorName { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ProductionRepository _productionRepository;
    private readonly AlarmRepository _alarmRepository;
    private readonly ProcessLogRepository _processLogRepository; // YENİ: ProcessLogRepository tanımlandı
    private readonly MachineRepository _machineRepository;
    private readonly DashboardRepository _dashboardRepository;
    private readonly UserRepository _userRepository;

    // Constructor güncellenmeli
    public ReportsController(ProductionRepository productionRepository, AlarmRepository alarmRepository, ProcessLogRepository processLogRepository,MachineRepository machineRepository,DashboardRepository dashboardRepository,UserRepository userRepository)
    {
        _productionRepository = productionRepository;
        _alarmRepository = alarmRepository;
        _processLogRepository = processLogRepository; // YENİ atama
        _machineRepository = machineRepository;
        _dashboardRepository = dashboardRepository;
        _userRepository = userRepository;
    }

    [HttpPost("production")]
    public IActionResult GetProductionReport([FromBody] ReportFiltersDto filtersDto)
    {
        // Kontrol ekleme: Gelen tarihlerin null olup olmadığını kontrol ediyoruz.
        if (filtersDto.StartTime == null || filtersDto.EndTime == null)
        {
            // Eğer StartTime veya EndTime null ise Bad Request döndür.
            return BadRequest("Başlangıç ve Bitiş tarihleri zorunludur.");
        }

        try
        {
            // Tarih çevrimini önceki adımdaki gibi güçlü tutuyoruz.
            var coreFilters = new ReportFilters
            {
                StartTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                EndTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),

                // Diğer string alanları null olabilir, bu yüzden trimleyip gönderiyoruz.
                MachineId = filtersDto.MachineId,
                BatchNo = filtersDto.BatchNo?.Trim(),
                RecipeName = filtersDto.RecipeName?.Trim(),
                SiparisNo = filtersDto.SiparisNo?.Trim(),
                MusteriNo = filtersDto.MusteriNo?.Trim(),
                OperatorName = filtersDto.OperatorName?.Trim()
            };

            var reportData = _productionRepository.GetProductionReport(coreFilters);
            return Ok(reportData);
        }
        catch (Exception ex)
        {
            // Detaylı hata mesajı döndürerek debug'ı kolaylaştırıyoruz.
            return StatusCode(500, $"Rapor oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }
    // YENİ METOT: Alarm Raporu
    [HttpPost("alarms")]
    public ActionResult<IEnumerable<AlarmReportItem>> GetAlarmReport([FromBody] ReportFiltersDto filtersDto)
    {
        // Null kontrolü, 400 hatasını önlemek için WebApp'ten gelen verinin kontrolünü sağlar.
        if (filtersDto.StartTime == null || filtersDto.EndTime == null)
        {
            return BadRequest("Başlangıç ve Bitiş tarihleri zorunludur.");
        }

        try
        {
            var startTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // EndTime'ı repoda kullanacağımız `<` operatörüne hazırlamak için ertesi günün başlangıcını alıyoruz.
            var effectiveEndTime = endTime.Date.AddDays(1);

            var reportData = _alarmRepository.GetAlarmReport(startTime.Date, effectiveEndTime, filtersDto.MachineId);
            return Ok(reportData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Alarm raporu oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }

    [HttpPost("trend")]
    public IActionResult GetTrendData([FromBody] ReportFiltersDto filtersDto)
    {
        // Makine seçimi zorunlu olmalı
        if (filtersDto.StartTime == null || filtersDto.EndTime == null || filtersDto.MachineId == null || filtersDto.MachineId.Value == 0)
        {
            return BadRequest("Başlangıç ve Bitiş tarihleri ile Makine seçimi zorunludur.");
        }

        try
        {
            var startTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // EndTime bir sonraki günün başlangıcı olmalı
            var effectiveEndTime = endTime.Date.AddDays(1);
            int machineId = filtersDto.MachineId.Value;

            // ProcessLogRepository'deki GetLogsForDateRange metodunu çağırıyoruz
            var trendData = _processLogRepository.GetLogsForDateRange(machineId, startTime.Date, effectiveEndTime);

            // ProcessLogRepository.ProcessDataPoint listesini döndürüyoruz.
            return Ok(trendData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Trend verileri oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }
    [HttpPost("manual-consumption")]
    public ActionResult<ManualConsumptionSummary> GetManualConsumptionReport([FromBody] ReportFiltersDto filtersDto)
    {
        if (filtersDto.StartTime == null || filtersDto.EndTime == null || filtersDto.MachineId == null || filtersDto.MachineId.Value == 0)
        {
            return BadRequest("Başlangıç ve Bitiş tarihleri ile Makine seçimi zorunludur.");
        }

        try
        {
            var startTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // KRİTİK DÜZELTME 1: Başlangıç tarihi için saati sıfırla ve Kind'ı belirt.
            var coreStartTime = DateTime.SpecifyKind(startTime.Date, DateTimeKind.Unspecified);

            // KRİTİK DÜZELTME 2: Bitiş tarihi için tam olarak 23:59:59.9999999 (son tick) yap ve Kind'ı belirt.
            // Bu, Core'daki LogTimestamp BETWEEN @StartTime AND @EndTime sorgusunun doğru çalışmasını sağlar.
            var coreEndTime = DateTime.SpecifyKind(endTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);

            int machineId = filtersDto.MachineId.Value;

            // Makine adını bul
            var machine = _machineRepository.GetAllMachines().FirstOrDefault(m => m.Id == machineId);
            string machineName = machine?.MachineName ?? "Bilinmeyen Makine";

            // Core Repo'ya en hassas ve güvenli DateTime değerleri gönderilir.
            var summary = _processLogRepository.GetManualConsumptionSummary(machineId, machineName, coreStartTime, coreEndTime);

            if (summary == null)
            {
                // Core repo'nun bu noktada null döndürmesinin tek sebebi, ya GetManualLogs ya da GetManualLogs1'in boş gelmesidir.
                // Bu, Core repo'nun SQL sorgusunun bu parametrelerle bile veri bulamadığını gösterir.
                return NotFound("Seçilen makine ve tarih aralığı için manuel tüketim verisi bulunamadı.");
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Manuel tüketim raporu oluşturulurken bir hata oluştu: {ex.Message}");
        }
        // YENİ METOT: Genel Tüketim Toplamları
       

    }
    [HttpPost("consumption-totals")]
    public ActionResult<ConsumptionTotals> GetConsumptionTotals([FromBody] ReportFiltersDto filtersDto)
    {
        if (filtersDto.StartTime == null || filtersDto.EndTime == null)
        {
            return BadRequest("Başlangıç ve Bitiş tarihleri zorunludur.");
        }

        try
        {
            var startTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            var coreStartTime = DateTime.SpecifyKind(startTime.Date, DateTimeKind.Unspecified);
            var coreEndTime = DateTime.SpecifyKind(endTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);

            // KRİTİK DÜZELTME: _dashboardRepository yerine _productionRepository kullanılıyor
            var totals = _productionRepository.GetConsumptionTotalsForPeriod(coreStartTime, coreEndTime);

            if (totals.TotalWater == 0 && totals.TotalElectricity == 0 && totals.TotalSteam == 0)
            {
                return NotFound("Seçilen aralıkta tamamlanmış üretim verisi bulunamadı.");
            }

            return Ok(totals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Genel tüketim raporu oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }
    // YENİ METOT: Genel Detaylı Tüketim Raporu (Çoklu Makine)
    [HttpPost("general-detailed")]
    public ActionResult<IEnumerable<ProductionReportItem>> GetGeneralDetailedConsumptionReport([FromBody] GeneralDetailedConsumptionFilters filtersDto)
    {
        if (filtersDto.StartTime == null || filtersDto.EndTime == null || filtersDto.MachineIds == null || !filtersDto.MachineIds.Any())
        {
            return BadRequest("Zaman aralığı ve makine seçimi zorunludur.");
        }

        try
        {
            var startTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            var coreStartTime = DateTime.SpecifyKind(startTime.Date, DateTimeKind.Unspecified);
            var coreEndTime = DateTime.SpecifyKind(endTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);

            List<ProductionReportItem> combinedResults = new List<ProductionReportItem>();

            // Her makine ID'si için ayrı ayrı rapor çekeriz (Çoklu seçim desteği için tek tek çağırma)
            foreach (var machineId in filtersDto.MachineIds)
            {
                var singleMachineFilter = new ReportFilters
                {
                    StartTime = coreStartTime,
                    EndTime = coreEndTime,
                    MachineId = machineId
                };

                var machineReport = _productionRepository.GetProductionReport(singleMachineFilter);

                // Sadece tüketim verisi olan tamamlanmış partileri rapora ekleriz
                combinedResults.AddRange(machineReport.Where(item =>
                    item.EndTime != DateTime.MinValue));
            }

            return Ok(combinedResults);

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Genel detaylı tüketim raporu oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }
    [HttpPost("action-logs")]
    public ActionResult<IEnumerable<ActionLogEntry>> GetActionLogs([FromBody] ActionLogFiltersDto filtersDto)
    {
        if (filtersDto.StartTime == null || filtersDto.EndTime == null)
        {
            return BadRequest("Başlangıç ve Bitiş tarihleri zorunludur.");
        }

        try
        {
            var startTime = DateTime.Parse(filtersDto.StartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(filtersDto.EndTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            var coreStartTime = DateTime.SpecifyKind(startTime.Date, DateTimeKind.Unspecified);
            var coreEndTime = DateTime.SpecifyKind(endTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);

            var reportData = _userRepository.GetActionLogs(coreStartTime, coreEndTime, filtersDto.Username?.Trim(), filtersDto.Details?.Trim());
            return Ok(reportData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Eylem Kayıtları raporu oluşturulurken bir hata oluştu: {ex.Message}");
        }
    }
}