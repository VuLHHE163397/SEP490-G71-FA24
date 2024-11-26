using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{
    public class ServiceController : Controller
    {
        private readonly HttpClient client;
        private string ServiceApiUri = "https://localhost:7056/api/Service";
        private string RoomApiUri = "https://localhost:7056/api/Room";
        private string BuildingApiUri = "https://localhost:7056/api/Building";

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
            return View();
        }
        public IActionResult AddService()
        {
            return View();
        }
        public IActionResult EditService(int id, string token)
        {
            return View();
        }


        public IActionResult ServicesBills()
        {
            return View();
        }
        public IActionResult ServiceRecord()
        {
            return View();
        }
    }
}
