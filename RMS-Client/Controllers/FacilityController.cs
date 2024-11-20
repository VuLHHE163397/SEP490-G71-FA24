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
        public async Task<IActionResult> ListFacility(string? keyword, int buildingId, int roomId, int pageIndex = 1, int pageSize = 10)
        {
            string apiUrlFacility = FacilityApiUri + $"/GetAllFacility?pageIndex={pageIndex}&pageSize={pageSize}";
            if(keyword != null)
            {
                apiUrlFacility += $"&keyword={keyword}";
            }
            if(buildingId > 0)
            {
                apiUrlFacility += $"&buildingId={buildingId}";
            }
            if(roomId > 0)
            {
                apiUrlFacility += $"&roomId={roomId}";
            }
            var facilityResponse = await client.GetAsync(apiUrlFacility);
            var tableView = new FacilityTableView();
            if (facilityResponse.IsSuccessStatusCode)
            {
                var json = await facilityResponse.Content.ReadAsStringAsync();
                tableView = JsonConvert.DeserializeObject<FacilityTableView>(json) ?? new FacilityTableView();
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
            ViewBag.TotalRecord = tableView.Total;
            return View(tableView.Facilities);
        }
        public IActionResult AddFacility()
        {
            return View();
        }
        public async Task<IActionResult> EditFacility(int id)
        {
            string apiUrlFacility = $"{FacilityApiUri}?id={id}";
            var facilityResponse = await client.GetAsync(apiUrlFacility);
            var facilities = new FacilityDTO();
            if (facilityResponse.IsSuccessStatusCode)
            {
                var json = await facilityResponse.Content.ReadAsStringAsync();
                facilities = JsonConvert.DeserializeObject<FacilityDTO>(json) ?? new FacilityDTO();
            }
            return View(facilities);
        }
    }
}
