// Dosya: TekstilScada.WebAPI/Controllers/ReportsController.cs

using Microsoft.AspNetCore.Mvc;
using TekstilScada.Repositories;

namespace TekstilScada.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ProductionRepository _productionRepository;

        public ReportsController(ProductionRepository productionRepository)
        {
            _productionRepository = productionRepository;
        }

        /// <summary>
        /// Belirtilen filtrelere göre üretim raporu verilerini döndürür.
        /// POST: /api/reports/production
        /// </summary>
        [HttpPost("production")]
        public IActionResult GetProductionReport([FromBody] ReportFilters filters)
        {
            try
            {
                var reportData = _productionRepository.GetProductionReport(filters);
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Rapor oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }
    }
}