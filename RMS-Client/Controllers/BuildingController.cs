using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Newtonsoft.Json;
using RMS_API.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{

    public class BuildingController : Controller
    {

        private readonly HttpClient _client = null;
        private readonly string BuildingApiUri = "https://localhost:7056/api/Building";
        private readonly string GetBuildingById = "https://localhost:7056/api/Building/GetBuildingById";

        public BuildingController()
        {
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
        }
        
            public async Task<IActionResult> ListBuilding()
        {
            string apiUrlBuilding = BuildingApiUri + "/GetAllBuildings";
            var buildings = new List<BuildingDTO>();
            var responseBuilding = await _client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<BuildingDTO>>(json);
            }
            return View(buildings);
        }
        public async Task<IActionResult> EditBuilding(int? id)
        {
            string apiUrlGetBuildingById = GetBuildingById + "/id";
            var buildings = new BuildingDTO();
            var res = await _client.GetAsync(apiUrlGetBuildingById);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<BuildingDTO>(json);
            }
            return View(buildings);
        }
    }
}
