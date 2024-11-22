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
        public async Task<IActionResult> ListService(string? keyword, int buildingId)
        {
            string apiUrlService = ServiceApiUri + "/GetAllService";
            var serviceResponse = await client.GetAsync(apiUrlService);
            var services = new ServiceTableView();
            if(serviceResponse.IsSuccessStatusCode)
            {
                var json = await serviceResponse.Content.ReadAsStringAsync();
                services = JsonConvert.DeserializeObject<ServiceTableView>(json) ?? new ServiceTableView();
            }
            // Lấy danh sách tòa nhà và trạng thái (như trước đây)
            string apiUrlBuilding = RoomApiUri + "/GetAllBuilding";
            var buildings = new List<Building>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<Building>>(json);
            }

            string apiUrlStatusRo = RoomApiUri + "/GetAllStatus";
            var status = new List<RoomStatus>();
            var responseStatusRo = await client.GetAsync(apiUrlStatusRo);
            if (responseStatusRo.IsSuccessStatusCode)
            {
                var json = await responseStatusRo.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<RoomStatus>>(json);
            }
            ViewBag.Buildings = buildings;
            ViewBag.Status = status;
            ViewBag.TotalRecord = services.TotalRecord;


            return View(services.services);
        }
        public IActionResult AddService()
        {
            return View();
        }
        public async Task<IActionResult> EditService(int id)
        {
            string apiUrlService = $"{ServiceApiUri}?id={id}";
            var serviceResponse = await client.GetAsync(apiUrlService);
            var service = new ServiceDTO();
            if (serviceResponse.IsSuccessStatusCode)
            {
                var json = await serviceResponse.Content.ReadAsStringAsync();
                service = JsonConvert.DeserializeObject<ServiceDTO>(json) ?? new ServiceDTO();
                // Làm tròn giá trị để loại bỏ phần thập phân
                service.Price = Math.Round(service.Price, 0);
            }
            string apiUrlBuilding = RoomApiUri + "/GetAllBuilding";
            var buildings = new List<Building>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<Building>>(json);
            }
            ViewBag.Buildings = buildings;
            return View(service);
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
