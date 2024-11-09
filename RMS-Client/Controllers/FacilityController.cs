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
        public async Task<IActionResult> ListFacility()
        {
            string apiUrlFacility = FacilityApiUri + "/GetAllFacility";
            var facilityResponse = await client.GetAsync(apiUrlFacility);
            var facilities = new List<FacilityDTO>();
            if (facilityResponse.IsSuccessStatusCode)
            {
                var json = await facilityResponse.Content.ReadAsStringAsync();
                facilities = JsonConvert.DeserializeObject<List<FacilityDTO>>(json) ?? new List<FacilityDTO>();
            }
            return View(facilities);
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
