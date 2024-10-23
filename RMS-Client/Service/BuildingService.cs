using Newtonsoft.Json;
using RMS_API.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RMS_API.Models;

namespace RMS_Client.Services
{
    public class BuildingService
    {
        private readonly HttpClient _httpClient;

        public BuildingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<BuildingDTO>> GetAllBuildingsAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:7056/api/Building/GetAllBuildings");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<BuildingDTO>>(jsonResponse);
        }
    }
}
