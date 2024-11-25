using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{

    public class FacilityController : Controller
    {
       

        private readonly HttpClient client;
        private string FacilityApiUri = "https://localhost:7056/api/Facility";
        private string RoomApiUri = "https://localhost:7056/api/Room";

        public FacilityController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ListFacility()
        {
            return View();
        }
        public IActionResult AddFacility()
        {
            return View();
        }
        public IActionResult EditFacility(int id, string token)
        {
            return View();
        }
    }
}
