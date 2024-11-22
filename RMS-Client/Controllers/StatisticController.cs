using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RMS_Client.Controllers
{
    public class StatisticController : Controller
    {
        private readonly ILogger<StatisticController> _logger;
        public StatisticController(ILogger<StatisticController> logger)
        {
            _logger = logger;
        }
        public IActionResult ViewStatistic()
        {
            return View("~/Views/Statistic/ViewStatistic.cshtml");
        }
    }
}
