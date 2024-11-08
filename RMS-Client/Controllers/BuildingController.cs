using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{

    public class BuildingController : Controller
    {

        private readonly HttpClient _client = null;
        private readonly string BuildingApiUri = "https://localhost:7056/api/Building";
        private readonly string GetBuildingById = "https://localhost:7056/api/Building/GetBuildingById";
        private readonly string GetDistrictsByProvince = "https://localhost:7056/api/Building/GetDistrictsByProvince";
        private readonly string GetBuildinImformationgById = "https://localhost:7056/api/Building/GetBuildinImformationgById";
        private readonly string GetBuildingStatus = "https://localhost:7056/api/Building/GetBuildingStatus";
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


        public async Task<IActionResult> BuildingDetail(int? id)
        {
            if (id == null)
            {
                return BadRequest("Building ID is required");
            }


            string apiUrlGetBuildingById = $"{GetBuildinImformationgById}/{id.Value}";
            var building = new BuildingDTO();

            try
            {
                var res = await _client.GetAsync(apiUrlGetBuildingById);
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    building = JsonConvert.DeserializeObject<BuildingDTO>(json);
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Unable to retrieve building by id.");
                    return View(building);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(building);
            }

            // Pass the building object to the view
            return View(building);
        }

        
        public async Task<IActionResult> EditBuilding(int? id)
        {


            if (id == null)
            {
                return BadRequest("Building ID is required");
            }

            
            string apiUrlGetBuildingById = $"{GetBuildingById}/{id.Value}";
            var building = new BuildingDTO();

            try
            {
                var res = await _client.GetAsync(apiUrlGetBuildingById);
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    building = JsonConvert.DeserializeObject<BuildingDTO>(json);
                }
                else
                {
                    
                    ModelState.AddModelError(string.Empty, "Unable to retrieve building by id.");
                    return View(building);  
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(building);
            }
           

            return View(building);
           
        }
     
      /*  public async Task<IActionResult> EditBuilding()
        {
            

            // Lấy status của room
            string apiUrlStatusBu = GetBuildingStatus;
            HttpResponseMessage statusResponse = await _client.GetAsync(apiUrlStatusBu);
            if (statusResponse.IsSuccessStatusCode)
            {
                var statusData = await statusResponse.Content.ReadAsStringAsync();
                var status = JsonConvert.DeserializeObject<List<BuildingStatus>>(statusData);
                ViewBag.Status = status;
            }
            return View(); // Truyền đối tượng RoomDTO đã đặt mặc định



        }*/






    }
}
