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
                // Gọi API đếm tổng số phòng
                var roomsResponse = await _httpClient.GetAsync("api/statistic/count-rooms");
                var roomsResult = JsonConvert.DeserializeObject<dynamic>(await roomsResponse.Content.ReadAsStringAsync());
                ViewBag.TotalRooms = roomsResult?.totalRooms ?? 0;

                // Gọi API đếm tổng số người dùng roleId = 2
                var usersResponse = await _httpClient.GetAsync("api/statistic/count-users-with-role2");
                var usersResult = JsonConvert.DeserializeObject<dynamic>(await usersResponse.Content.ReadAsStringAsync());
                ViewBag.TotalUsersWithRole2 = usersResult?.totalUsers ?? 0;

                // Gọi API đếm tổng số tòa nhà
                var buildingsResponse = await _httpClient.GetAsync("api/statistic/count-buildings");
                var buildingsResult = JsonConvert.DeserializeObject<dynamic>(await buildingsResponse.Content.ReadAsStringAsync());
                ViewBag.TotalBuildings = buildingsResult?.totalBuildings ?? 0;

                // Truyền danh sách phòng vào View nếu cần
                var rooms = await _httpClient.GetFromJsonAsync<List<RoomDTO>>("api/Room/GetActiveRooms");
                if (rooms == null)
                {
                    _logger.LogWarning("No rooms were retrieved from the API.");
                    return View("Home", new List<RoomDTO>());
                }

                return View("Home", rooms); // Truyền rooms nếu cần sử dụng trong View
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching data.");
                return View("Error");
            }
        }

        public async Task<IActionResult> RoomDetail(int id)
        {
            try
            {
                // Gọi API chi tiết phòng
                var roomDetailResponse = await _httpClient.GetAsync($"api/Room/detail/{id}");
                if (!roomDetailResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Room detail for ID {id} could not be retrieved. Status Code: {roomDetailResponse.StatusCode}");
                    return NotFound("Room detail could not be retrieved.");
                }

                var roomDetailJson = await roomDetailResponse.Content.ReadAsStringAsync();
                var roomDetail = JsonConvert.DeserializeObject<RoomDetailDTO>(roomDetailJson);
                if (roomDetail == null)
                {
                    _logger.LogWarning($"Room detail data is null for ID {id}.");
                    return NotFound("Room detail is missing.");
                }

                // Gọi API danh sách ảnh
                var imageResponse = await _httpClient.GetAsync($"api/Room/GetAllImage/{id}");
                if (!imageResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Images for Room ID {id} could not be retrieved. Status Code: {imageResponse.StatusCode}");
                    roomDetail.Images = new List<ImageDTO>(); // Không có ảnh, gán danh sách rỗng
                }
                else
                {
                    var imageJson = await imageResponse.Content.ReadAsStringAsync();
                    var images = JsonConvert.DeserializeObject<List<ImageDTO>>(imageJson);
                    roomDetail.Images = images ?? new List<ImageDTO>();
                }

                // Gọi API phòng gợi ý
                var suggestedRoomsResponse = await _httpClient.GetAsync($"api/Room/{id}/suggestedrooms");
                var suggestedRooms = new List<SuggestedRoomDTO>();
                if (suggestedRoomsResponse.IsSuccessStatusCode)
                {
                    var suggestedRoomsJson = await suggestedRoomsResponse.Content.ReadAsStringAsync();
                    suggestedRooms = JsonConvert.DeserializeObject<List<SuggestedRoomDTO>>(suggestedRoomsJson) ?? new List<SuggestedRoomDTO>();
                }
                else
                {
                    _logger.LogWarning($"Suggested rooms for Room ID {id} could not be retrieved. Status Code: {suggestedRoomsResponse.StatusCode}");
                }

                // Truyền dữ liệu vào ViewData
                ViewData["RoomDetail"] = roomDetail;
                ViewData["SuggestedRooms"] = suggestedRooms;

                // Trả về view cùng tất cả thông tin
                return View(roomDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching data for Room ID {id}.");
                return View("Error");
            }
        }

        public IActionResult ListFavouriteRoom()
        {
            return View("~/Views/Home/ListFavouriteRoom.cshtml");
        }

        public IActionResult Login()
        {
            return View("~/Views/Login/Login.cshtml");
        }
        public IActionResult Search()
        {
            return View("~/Views/Home/SearchRoom.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}