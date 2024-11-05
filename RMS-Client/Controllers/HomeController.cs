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
                var response = await _httpClient.GetAsync($"api/Room/detail/{id}");

                // Kiểm tra nếu phản hồi không thành công
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Room detail for ID {id} could not be retrieved. Status Code: {response.StatusCode}");
                    return NotFound(); // Trả về trang 404 nếu không tìm thấy phòng
                }

                // Chuyển đổi JSON từ API thành RoomDetailDto
                var roomDetailJson = await response.Content.ReadAsStringAsync();
                var roomDetail = JsonConvert.DeserializeObject<RoomDetailDTO>(roomDetailJson);

                // Kiểm tra dữ liệu null trước khi truyền vào view
                if (roomDetail == null)
                {
                    _logger.LogWarning($"Room detail data is null for ID {id}.");
                    return NotFound(); // Trả về trang 404 nếu không có dữ liệu
                }

                // Truyền dữ liệu chi tiết phòng vào view RoomDetail.cshtml
                return View("~/Views/Home/RoomDetail.cshtml", roomDetail);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có ngoại lệ xảy ra
                _logger.LogError(ex, $"An error occurred while fetching room details for ID {id}.");
                return View("Error"); // Trả về trang Error nếu có lỗi
            }
        }

        public IActionResult ListFavouriteRoom()
        {
            return View("~/Views/Home/ListFavouriteRoom.cshtml");
        }
        public IActionResult Register()
        {
            return View("~/Views/Login/Register.cshtml");
        }
        public IActionResult Login()
        {
            return View("~/Views/Login/Login.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
