using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;
using RMS_API.DTOs;
using RMS_Client.Models;
using System.Diagnostics;

namespace RMS_Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7056"); // Cấu hình BaseAddress theo URL API của bạn
        }

        public async Task<IActionResult> Home()
        {
            try
            {
                // Gọi API GetActiveRooms và nhận danh sách RoomDTO
                var rooms = await _httpClient.GetFromJsonAsync<List<RoomDTO>>("api/Room/GetActiveRooms");

                // Kiểm tra xem dữ liệu có null không
                if (rooms == null)
                {
                    _logger.LogWarning("No rooms were retrieved from the API.");
                    return View("Home", new List<RoomDTO>()); // Trả về view rỗng nếu không có dữ liệu
                }

                // Truyền dữ liệu tới View
                return View("Home", rooms);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có ngoại lệ
                _logger.LogError(ex, "Error occurred while fetching rooms.");
                return View("Error"); // Bạn có thể tạo trang Error.cshtml để hiển thị lỗi
            }
        }


        public async Task<IActionResult> RoomDetail(int id)
        {
            try
            {
                // Gọi API chi tiết phòng dựa trên id
                var roomDetailResponse = await _httpClient.GetAsync($"api/Room/detail/{id}");

                if (!roomDetailResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Room detail for ID {id} could not be retrieved. Status Code: {roomDetailResponse.StatusCode}");
                    return NotFound(); // Nếu không có chi tiết phòng
                }

                var roomDetailJson = await roomDetailResponse.Content.ReadAsStringAsync();
                var roomDetail = JsonConvert.DeserializeObject<RoomDetailDTO>(roomDetailJson);

                // Kiểm tra dữ liệu chi tiết phòng
                if (roomDetail == null)
                {
                    _logger.LogWarning($"Room detail data is null for ID {id}.");
                    return NotFound();
                }

                // Gọi API phòng gợi ý dựa trên ID phòng
                var suggestedRoomsResponse = await _httpClient.GetAsync($"api/Room/{id}/suggestedrooms");

                if (!suggestedRoomsResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Suggested rooms for Room ID {id} could not be retrieved. Status Code: {suggestedRoomsResponse.StatusCode}");
                    return NotFound(); // Nếu không có phòng gợi ý
                }

                var suggestedRoomsJson = await suggestedRoomsResponse.Content.ReadAsStringAsync();
                var suggestedRooms = JsonConvert.DeserializeObject<List<SuggestedRoomDTO>>(suggestedRoomsJson);

                // Kiểm tra nếu có phòng gợi ý
                if (suggestedRooms == null || !suggestedRooms.Any())
                {
                    _logger.LogWarning($"No suggested rooms found for Room ID {id}.");
                    suggestedRooms = new List<SuggestedRoomDTO>(); // Đảm bảo không null
                }

                // Truyền cả dữ liệu chi tiết phòng và phòng gợi ý vào ViewData
                ViewData["RoomDetail"] = roomDetail;
                ViewData["SuggestedRooms"] = suggestedRooms;

                // Trả về view với cả dữ liệu chi tiết phòng và phòng gợi ý
                return View(roomDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching room details for ID {id}.");
                return View("Error"); // Nếu có lỗi xảy ra
            }
        }


        public IActionResult ListFavouriteRoom()
        {
            return View("~/Views/Home/ListFavouriteRoom.cshtml");
        }
      

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
