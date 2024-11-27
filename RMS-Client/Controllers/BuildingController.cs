using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using static Dropbox.Api.Files.SearchMatchType;

namespace RMS_Client.Controllers
{

    public class BuildingController : Controller
    {

        private readonly HttpClient _client = null;
        private readonly string BuildingApiUri = "https://localhost:7056/api/Building";
        private readonly string GetBuildingById = "https://localhost:7056/api/Building/GetBuildingById";
        private readonly string GetDistrictsByProvince = "https://localhost:7056/api/Building/GetDistrictsByProvince";
        private readonly string GetBuildinImformationgById = "https://localhost:7056/api/Building/GetBuildinImformationgById";
        private readonly RMS_SEP490Context _context;
        public BuildingController(RMS_SEP490Context context)
        {
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            _context = context;
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

        [HttpGet("AddBuilding")]
        public IActionResult AddBuilding()
        {
            // Lấy danh sách các tỉnh
            ViewBag.Provinces = _context.Provinces.ToList();

            // Lấy danh sách trạng thái tòa nhà
            ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();

            return View(); // Trả về view cho việc thêm tòa nhà
        }

        [HttpPost]
        
        public async Task<IActionResult> AddBuilding(BuildingDTO buildingDTO)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                // If model is invalid, reload necessary data for the dropdowns and return the view
                ViewBag.Provinces = _context.Provinces.ToList();
                ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();
                return View(buildingDTO);
            }

            try
            {
                // Send the building data to the API to add it
                var jsonContent = JsonConvert.SerializeObject(buildingDTO);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Post the data to the API
                var response = await _client.PostAsync(BuildingApiUri + "/AddBuildingbyId", content);

                if (response.IsSuccessStatusCode)
                {
                    // If successful, redirect to ListBuilding or show a success message
                    return RedirectToAction("ListBuilding");
                }
                else
                {
                    // If unsuccessful, add a model error and return the view
                    ModelState.AddModelError(string.Empty, "Unable to add building.");
                    ViewBag.Provinces = _context.Provinces.ToList();
                    ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();
                    return View(buildingDTO);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors during the API call or processing
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                ViewBag.Provinces = _context.Provinces.ToList();
                ViewBag.BuildingStatuses = _context.BuildingStatuses.ToList();
                return View(buildingDTO);
            }
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

       /* public async Task<IActionResult> AdddBuilding()
        {
            return Ok();
        }*/



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
            //lay status building
            /*string apiUrlStatusBu = GetDistrictsByProvince + "/provinceName";
            var status = new BuildingStatus();
            var resp = await _client.GetAsync(apiUrlStatusBu);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<BuildingStatus>(json);
            }
            ViewBag.Status = status;
*/
            return View(building);





        }





    }
}