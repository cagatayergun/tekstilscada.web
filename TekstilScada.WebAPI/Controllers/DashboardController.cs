using Microsoft.AspNetCore.Mvc;
using TekstilScada.Repositories;
using TekstilScada.Models; // OeeData için eklendi
using System;
using System.Collections.Generic;

namespace TekstilScada.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardRepository _dashboardRepository;

        public DashboardController(DashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        [HttpGet("oee-report")]
        public ActionResult<IEnumerable<OeeData>> GetOeeReport([FromQuery] DateTime startTime, [FromQuery] DateTime endTime, [FromQuery] int? machineId)
        {
            try
            {
                var reportData = _dashboardRepository.GetOeeReport(startTime, endTime, machineId);
                return Ok(reportData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"OEE Raporu oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }
    }
}