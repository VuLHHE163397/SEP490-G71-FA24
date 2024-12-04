using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{
    public class MaintainanceController : Controller
    {
        private readonly HttpClient _httpClient;
        private string MaintainanceApiUri = "https://localhost:7056/api/Maintainance";

        public MaintainanceController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);

        }

        public IActionResult ListMaintainance()
        {
            return View();
        }
    }
}