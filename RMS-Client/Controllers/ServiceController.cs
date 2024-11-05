using Microsoft.AspNetCore.Mvc;
using RMS_API.DTOs;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{
    public class ServiceController : Controller
    {
        private readonly HttpClient client = null;
        private string ServiceApiUri = "https://localhost:7056/api/Service";
        

        public ServiceController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ListService()
        {
            string apiUrlService = ServiceApiUri + "/GetAllServie";
            var buildings = new List<ServiceDTO>();
            return View();
        }
        public IActionResult AddService()
        {
            return View();
        }
        public IActionResult EditService()
        {
            return View();
        }
    }
}
