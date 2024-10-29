using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient client = null;
        private string RoomApiUri = "https://localhost:7056/api/Room";
        private string BuildingApiUri = "https://localhost:7056/api/Building";
        private string RoomStatusApiUri = "https://localhost:7056/api/RoomStatus";

        public RoomController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);

        }

        public async Task<IActionResult> ListRoom(int? statusId, int? buildingId)
        {
            string apiUrl = RoomApiUri + "/GetAllRoom";
            var rooms = new List<Room>();

            // Gọi API lấy danh sách phòng
            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                rooms = JsonConvert.DeserializeObject<List<Room>>(json);
            }

            // Lọc danh sách phòng theo BuildingId
            if (buildingId.HasValue)
            {
                rooms = rooms.Where(r => r.BuildingId == buildingId.Value).ToList();
            }

            // Lấy danh sách tòa nhà
            string apiUrlBuilding = BuildingApiUri + "/GetAllBuildings";
            var buildings = new List<BuildingDTO>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<BuildingDTO>>(json);
            }

            //Lấy status của room
            string apiUrlStatusRo = RoomStatusApiUri + "/GetAllStatus";
            var status = new List<RoomStatus>();
            var responseStatusRo = await client.GetAsync(apiUrlStatusRo);
            if (responseStatusRo.IsSuccessStatusCode)
            {
                var json = await responseStatusRo.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<RoomStatus>>(json);
            }

            // Truyền cả rooms và buildings vào ViewBag
            ViewBag.Rooms = rooms;
            ViewBag.Buildings = buildings;
            ViewBag.Status = status;

            // Truyền dữ liệu qua view
            return View(rooms);
        }

    }
}
