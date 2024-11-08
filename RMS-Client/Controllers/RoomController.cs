using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using RMS_API.DTOs;
using RMS_API.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace RMS_Client.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient client = null;
        private string RoomApiUri = "https://localhost:7056/api/Room";

        public RoomController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        public async Task<IActionResult> ListRoom(List<int> statusIds, int? buildingId)
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

            // Lọc theo nhiều status
            if (statusIds != null && statusIds.Any())
            {
                rooms = rooms.Where(r => statusIds.Contains(r.RoomStatusId)).ToList();
            }

            // Lấy danh sách tòa nhà và trạng thái (như trước đây)
            string apiUrlBuilding = RoomApiUri + "/GetAllBuilding";
            var buildings = new List<Building>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<Building>>(json);
            }

            string apiUrlStatusRo = RoomApiUri + "/GetAllStatus";
            var status = new List<RoomStatus>();
            var responseStatusRo = await client.GetAsync(apiUrlStatusRo);
            if (responseStatusRo.IsSuccessStatusCode)
            {
                var json = await responseStatusRo.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<RoomStatus>>(json);
            }

            // Truyền ViewBag
            ViewBag.Buildings = buildings;
            ViewBag.Status = status;
            ViewBag.SelectedStatusIds = statusIds; // Lưu trữ trạng thái đã chọn
            ViewBag.SelectedBuildingId = buildingId; // Lưu trữ BuildingId đã chọn

            return View(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int? buildingId, List<int> statusIds)
        {
            // Gọi API để xóa phòng
            string apiUrl = $"{RoomApiUri}/DeleteRoomById/{id}";
            var response = await client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Sau khi xóa, giữ lại các tham số lọc và chuyển hướng về danh sách phòng
                return RedirectToAction("ListRoom", new { buildingId = buildingId, statusIds = statusIds });
            }
            return NotFound("Room could not be deleted.");
        }



        [HttpGet]
        // Hành động hiển thị trang thêm phòng
        public async Task<IActionResult> CreateRoom()
        {
            //// Tạo một đối tượng RoomDTO với RoomStatusId mặc định là 1
            //var roomDTO = new RoomDTO
            //{
            //    RoomStatusId = 1 // Đặt mặc định là 1 (Trạng thái "Trống")
            //};

            // Lấy danh sách tòa nhà
            string apiUrlBuilding = RoomApiUri + "/GetAllBuilding";
            var buildings = new List<Building>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<Building>>(json);
            }

            // Lấy status của room
            string apiUrlStatusRo = RoomApiUri + "/GetAllStatus";
            var status = new List<RoomStatus>();
            var responseStatusRo = await client.GetAsync(apiUrlStatusRo);
            if (responseStatusRo.IsSuccessStatusCode)
            {
                var json = await responseStatusRo.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<RoomStatus>>(json);
            }

            ViewBag.Buildings = buildings;
            ViewBag.Status = status;
            return View(); // Truyền đối tượng RoomDTO đã đặt mặc định
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(RoomLlDTO roomDTO)
        {
            if (ModelState.IsValid)
            {
                // Nếu RoomStatusId không có giá trị, gán mặc định là 1 (Trạng thái "Trống")
                roomDTO.RoomStatusId = roomDTO.RoomStatusId != 0 ? roomDTO.RoomStatusId : 1;

                var json = JsonConvert.SerializeObject(roomDTO);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(RoomApiUri + "/AddRoom", content);

                if (response.IsSuccessStatusCode)
                {
                    // Điều hướng đến danh sách phòng sau khi thêm thành công
                    return RedirectToAction("ListRoom");
                }
                else
                {
                    // In ra mã trạng thái và nội dung phản hồi
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {response.StatusCode}, {responseContent}");
                    // Thêm lỗi vào ModelState nếu thêm không thành công
                    ModelState.AddModelError("", "Thêm phòng không thành công.");
                }
            }

            // Nếu có lỗi hoặc ModelState không hợp lệ, trả về lại View với dữ liệu đã nhập
            return View(roomDTO);
        }



        public async Task<IActionResult> ExportToExcel(int buildingId)
        {
            var rooms = new List<Room>();
            string buildingName = "Building"; // Khởi tạo biến buildingName với giá trị mặc định

            // Gọi API để lấy tên tòa nhà
            var buildingResponse = await client.GetAsync(RoomApiUri + $"/GetBuildingById?buildingId={buildingId}");
            if (buildingResponse.IsSuccessStatusCode)
            {
                var buildingNameJson = await buildingResponse.Content.ReadAsStringAsync();
                buildingName = JsonConvert.DeserializeObject<string>(buildingNameJson) ?? "Building"; // Chuyển đổi chuỗi JSON và gán trực tiếp vào biến buildingName
            }
            else
            {
                return Content("Không thể kết nối đến API để lấy tên tòa nhà.");
            }


            // Gọi API với buildingId để lấy danh sách phòng
            var response = await client.GetAsync(RoomApiUri + $"/GetRoomByBuilding?buildingId={buildingId}");
            if (!response.IsSuccessStatusCode)
            {
                return Content("Không thể kết nối đến API.");
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json) || json == "[]")
            {
                return Content("Không có dữ liệu để xuất.");
            }

            rooms = JsonConvert.DeserializeObject<List<Room>>(json);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Rooms");

                worksheet.Cells[1, 1].Value = "Room Number";
                worksheet.Cells[1, 2].Value = "Area (m²)";
                worksheet.Cells[1, 3].Value = "Floor";
                worksheet.Cells[1, 4].Value = "Price (VNĐ)";
                worksheet.Cells[1, 5].Value = "Status";
                worksheet.Cells[1, 6].Value = "Description";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int row = 2;
                foreach (var room in rooms)
                {
                    worksheet.Cells[row, 1].Value = room.RoomNumber;
                    worksheet.Cells[row, 2].Value = room.Area;
                    worksheet.Cells[row, 3].Value = room.Floor;
                    worksheet.Cells[row, 4].Value = room.Price.ToString("N0") + " VNĐ";
                    worksheet.Cells[row, 5].Value = room.RoomStatusId switch
                    {
                        1 => "Trống",
                        2 => "Đã có người",
                        3 => "Đang sửa chữa",
                        4 => "Sắp trống",
                        _ => "Không xác định"
                    };
                    worksheet.Cells[row, 6].Value = room.Description;

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Danh sách tòa nhà {buildingName}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

    }
}
