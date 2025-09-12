using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
//using TekstilScada.Core.Repositories;
using TekstilScada.Core.Services;
using TekstilScada.Repositories;
using TekstilScada.Services;


namespace TekstilScada.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardRepository _dashboardRepository;
        private readonly MachineRepository _machineRepository;
        private readonly AlarmRepository _alarmRepository;
        private readonly PlcPollingService _pollingService;

        public DashboardController(DashboardRepository dashboardRepository, MachineRepository machineRepository, AlarmRepository alarmRepository, PlcPollingService pollingService)
        {
            _dashboardRepository = dashboardRepository;
            _machineRepository = machineRepository;
            _alarmRepository = alarmRepository;
            _pollingService = pollingService;
        }

       
    }
}
