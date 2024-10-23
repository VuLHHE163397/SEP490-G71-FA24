using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RMS_API.Models;
using System.Net.Http.Headers;

namespace RMS_Client.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient client = null;
        private string RoomApiUri = "https://localhost:7056/api/Room";
        private string RoomStatusApiUri = "https://localhost:7056/api/RoomStatus";

        public RoomController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);

        }

        public async Task<IActionResult> ListRoom(int? statusId, int? buildingId)
        {
            List<Room> rooms = new List<Room>();

            HttpResponseMessage response = await client.GetAsync($"{RoomApiUri}/GetAllRoom");
            if (response.IsSuccessStatusCode)
            {
                // Sử dụng ReadAsStringAsync để đọc nội dung JSON
                var jsonString = await response.Content.ReadAsStringAsync();

                // Deserialize JSON thành danh sách Room
                rooms = JsonConvert.DeserializeObject<List<Room>>(jsonString);
            }

            // Lọc theo trạng thái phòng nếu có
            if (statusId.HasValue)
            {
                rooms = rooms.Where(r => r.RooomStatusId == statusId.Value).ToList();
            }

            // Lọc theo tòa nhà nếu có
            if (buildingId.HasValue)
            {
                rooms = rooms.Where(r => r.BuildingId == buildingId.Value).ToList();
            }

            // Truyền các giá trị filter hiện tại tới View
            ViewBag.StatusId = statusId;
            ViewBag.BuildingId = buildingId;

            return View(rooms); // Truyền danh sách phòng vào View
        }

    }
}
