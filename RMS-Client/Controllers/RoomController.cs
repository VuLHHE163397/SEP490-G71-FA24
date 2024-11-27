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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dropbox.Api;
using Dropbox.Api.Files;
using static Dropbox.Api.Files.ListRevisionsMode;
using RMS_Client.Models;


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
            ViewBag.RoomStatuses = new SelectList(status, "Id", "Name");
            ViewBag.SelectedStatusIds = statusIds; // Lưu trữ trạng thái đã chọn
            ViewBag.SelectedBuildingId = buildingId; // Lưu trữ BuildingId đã chọn

            return View(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> RoomDetail(int id)
        {
            string apiUrl = $"{RoomApiUri}/GetRoomById/{id}";
            var room = new Room();

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                room = JsonConvert.DeserializeObject<Room>(json);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy thông tin phòng.";
                return RedirectToAction("ListRoom");
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

            // Lấy danh sách cơ sở vật chất theo RoomId
            string apiUrlFac = $"{RoomApiUri}/GetFacilityByRoomId/{id}";
            var facs = new List<Facility>();
            var responseFac = await client.GetAsync(apiUrlFac);
            if (responseFac.IsSuccessStatusCode)
            {
                var json = await responseFac.Content.ReadAsStringAsync();
                facs = JsonConvert.DeserializeObject<List<Facility>>(json);
            }

            string apiUrlImage = $"{RoomApiUri}/GetAllImage/{id}";
            var images = new List<Image>();
            var responseImage = await client.GetAsync(apiUrlImage);
            if (responseImage.IsSuccessStatusCode)
            {
                var json = await responseImage.Content.ReadAsStringAsync();
                images = JsonConvert.DeserializeObject<List<Image>>(json);
            }

            ViewBag.Images = images;
            ViewBag.Buildings = buildings;
            ViewBag.Facilities = facs;
            ViewBag.BuildingId = room?.BuildingId;
            return View(room);
        }


        [HttpGet]
        public async Task<IActionResult> CreateRoom(int? buildingId)
        {

            // Lấy danh sách Building từ API
            string apiUrlBuilding = RoomApiUri + "/GetAllBuilding";
            var buildingss = new List<BuildingDTO>();
            var responseBuildings = await client.GetAsync(apiUrlBuilding);
            if (responseBuildings.IsSuccessStatusCode)
            {
                var json = await responseBuildings.Content.ReadAsStringAsync();
                buildingss = JsonConvert.DeserializeObject<List<BuildingDTO>>(json);
            }

            string apiUrlStatusRo = RoomApiUri + "/GetAllStatus";
            var status = new List<RoomStatus>();
            var responseStatusRo = await client.GetAsync(apiUrlStatusRo);
            if (responseStatusRo.IsSuccessStatusCode)
            {
                var json = await responseStatusRo.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<RoomStatus>>(json);
            }

            // Lấy danh sách tòa nhà để hiển thị bên aside
            var buildings = new List<Building>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<Building>>(json);
            }


            ViewBag.RoomStatuses = new SelectList(status, "Id", "Name");

            ViewBag.Buildingss = new SelectList(buildingss, "Id", "Name");

            ViewBag.Buildings = buildings;

            ViewBag.SelectedBuildingId = buildingId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(RoomLlDTO roomDTO, int? buildingId)
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
                    return RedirectToAction("ListRoom", "Room", new { buildingId = buildingId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Thêm phòng không thành công. Chi tiết lỗi: {errorContent}");
                }
            }
            return View(roomDTO);
        }

        [HttpGet]
        public async Task<IActionResult> CheckRoomNameExists(int roomName, int buildingId)
        {
            string apiUrl = $"{RoomApiUri}/CheckRoomNameExists?roomName={roomName}&buildingId={buildingId}";
            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var exists = await response.Content.ReadAsStringAsync();
                return Json(bool.Parse(exists)); // Trả về true nếu phòng đã tồn tại, false nếu chưa
            }
            return Json(false);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoom(int id)
        {
            // Lấy thông tin phòng qua API dựa trên Id
            string apiUrl = $"{RoomApiUri}/GetRoomById/{id}";
            RoomLlUpdateDTO room = null;

            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                room = JsonConvert.DeserializeObject<RoomLlUpdateDTO>(json);
            }
            else
            {
                TempData["Error"] = "Không tìm thấy thông tin phòng.";
                return RedirectToAction("ListRoom");
            }

            string apiUrlStatusRo = RoomApiUri + "/GetAllStatus";
            var status = new List<RoomStatus>();
            var responseStatusRo = await client.GetAsync(apiUrlStatusRo);
            if (responseStatusRo.IsSuccessStatusCode)
            {
                var json = await responseStatusRo.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<RoomStatus>>(json);
            }

            string apiUrlBuilding = RoomApiUri + "/GetAllBuilding";
            var buildings = new List<Building>();
            var responseBuilding = await client.GetAsync(apiUrlBuilding);
            if (responseBuilding.IsSuccessStatusCode)
            {
                var json = await responseBuilding.Content.ReadAsStringAsync();
                buildings = JsonConvert.DeserializeObject<List<Building>>(json);
            }

            ViewBag.RoomStatuses = new SelectList(status, "Id", "Name");
            ViewBag.Buildings = buildings;
            // Lấy buildingId từ room và truyền vào ViewBag
            ViewBag.BuildingId = room?.BuildingId;
            return View(room);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoom(RoomLlUpdateDTO room, int? buildingId)
        {
            if (!ModelState.IsValid)
            {
                return View(room); // Trả lại view nếu model không hợp lệ
            }

            // Chuẩn bị dữ liệu JSON để gửi đi
            var jsonContent = JsonConvert.SerializeObject(room);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Gọi API PUT để cập nhật phòng
            string apiUrl = $"{RoomApiUri}/UpdateRoom";
            var response = await client.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật phòng thành công!";
                return RedirectToAction("ListRoom", new { buildingId = buildingId });
            }
            else
            {
                TempData["Error"] = "Cập nhật phòng thất bại: " + await response.Content.ReadAsStringAsync();
                return View(room); // Trả lại view với lỗi
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoomStatus(RoomLlUpdateDTO room, int? buildingId)
        {
            if (!ModelState.IsValid)
            {
                return View(room); // Trả lại view nếu model không hợp lệ
            }

            // Lấy chỉ statusId từ room DTO
            var jsonContent = JsonConvert.SerializeObject(room.RoomStatusId); // Gửi chỉ statusId
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string apiUrl = $"{RoomApiUri}/updateRoomStatus/{room.Id}";
            var response = await client.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật trạng thái phòng thành công!";
                return RedirectToAction("ListRoom", new { buildingId = buildingId });
            }
            else
            {
                TempData["Error"] = "Cập nhật phòng thất bại: " + await response.Content.ReadAsStringAsync();
                return Conflict("Cập nhật trạng thái phòng thất bại");
            }
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
            // Đọc thông báo lỗi từ phản hồi
            var errorMessage = await response.Content.ReadAsStringAsync();
            return NotFound($"Room could not be deleted. Error: {errorMessage}");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllRoomsByBuildingId(int buildingId)
        {
            var apiUrl = $"{RoomApiUri}/DeleteAllRoom/{buildingId}";
            var response = await client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Đã xóa tất cả phòng thuộc tòa nhà thành công.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa các phòng.";
            }

            // Chuyển hướng về trang ListRoom với buildingId sau khi xóa
            return RedirectToAction("ListRoom", new { buildingId = buildingId });
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
            var response = await client.GetAsync(RoomApiUri + $"/GetRoomByBuilding/{buildingId}");
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

                worksheet.Cells[1, 1].Value = "Số phòng";
                worksheet.Cells[1, 2].Value = "Diện tích";
                worksheet.Cells[1, 3].Value = "Tầng";
                worksheet.Cells[1, 4].Value = "Giá phòng";
                worksheet.Cells[1, 5].Value = "Trạng thái";
                worksheet.Cells[1, 6].Value = "Mô tả phòng";
                worksheet.Cells[1, 7].Value = "Ngày bắt đầu thuê phòng";
                worksheet.Cells[1, 8].Value = "Ngày hết hạn phòng thuê";
                worksheet.Cells[1, 9].Value = "Ngày phòng sẽ trống trong tương lai";

                using (var range = worksheet.Cells[1, 1, 1, 9])
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
                    worksheet.Cells[row, 4].Value = room.Price;
                    worksheet.Cells[row, 5].Value = room.RoomStatusId switch
                    {
                        1 => "Đang trống",
                        2 => "Đang cho thuê",
                        3 => "Đang bảo trì",
                        4 => "Sắp trống",
                        _ => "Không xác định"
                    };
                    worksheet.Cells[row, 6].Value = room.Description;
                    worksheet.Cells[row, 7].Value = room.StartedDate?.ToString("yyyy-MM-dd") ?? "Null";
                    worksheet.Cells[row, 8].Value = room.ExpiredDate?.ToString("yyyy-MM-dd") ?? "Null";
                    worksheet.Cells[row, 9].Value = room.FreeInFutureDate?.ToString("yyyy-MM-dd") ?? "Null";
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

        // Action để hiển thị form nhập liệu
        public IActionResult ImportRooms()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportRooms(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "No file uploaded.";
                return RedirectToAction("ImportRooms");
            }

            var formData = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            formData.Add(fileContent, "file", file.FileName);

            // Gửi yêu cầu POST đến API để nhập Rooms
            var response = await client.PostAsync($"{RoomApiUri}/ImportRooms", formData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Rooms imported successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while importing rooms.";
            }

            return RedirectToAction("ImportRooms");
        }
        public async Task<IActionResult> RoomMaintainance([FromRoute] int id)
        {
            string apiUrl = $"{RoomApiUri}/RoomMaintainance/{id}";
            var viewModel = new RoomQrViewModel();

            // Gửi request tới API
            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                viewModel = JsonConvert.DeserializeObject<RoomQrViewModel>(json);
            }

            return View(viewModel);
        }
<<<<<<< Updated upstream
        public async Task<IActionResult> SaveMaintenanceRequest(RoomQrViewModel model)
        {
            if (model == null)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu phòng không hợp lệ.");
                return View("RoomMaintainance", model);
            }

            var maintenanceDto = new MaintainanceDTO
            {
                Description = model.MaintenanceDescription,
                RequestDate = DateTime.Now,
                Status = 1,
                RoomId = model.Id
            };

            string apiUrl = $"{RoomApiUri}/SaveMaintenanceRequest";
            var content = new StringContent(JsonConvert.SerializeObject(maintenanceDto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.SuccessMessage = "Gửi báo cáo thành công! Nhấn OK để quay về trang chủ.";
                ViewBag.RedirectUrl = Url.Action("Home", "Home"); // URL của trang chủ
                return View("RoomMaintainance", model); // Giữ nguyên model trên form
            }

            ModelState.AddModelError(string.Empty, "Không thể gửi báo cáo. Vui lòng thử lại.");
            return View("RoomMaintainance", model);
        }

=======

        public async Task<IActionResult> SaveMaintenanceRequest(RoomQrViewModel model)
        {
            if (model != null)
            {
                // Tạo đối tượng gửi API
                var maintenanceDto = new MaintainanceDTO
                {
                    Description = model.MaintenanceDescription,
                    newDate = DateTime.Now,
                    Status = 1,
                    RoomId = model.Id
                };

                // Gửi API POST để lưu thông tin bảo trì
                string apiUrl = $"{RoomApiUri}/SaveMaintenanceRequest";
                var content = new StringContent(JsonConvert.SerializeObject(maintenanceDto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Báo cáo đã được gửi thành công! Bạn sẽ được chuyển hướng về Trang chủ sau 3s";
                    TempData["RedirectUrl"] = Url.Action("Home", "Home");
                    return RedirectToAction("RoomMaintainance"); // Trả về cùng view với TempData
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể gửi báo cáo. Vui lòng điền nội dung và thử lại");
                }
            }

            return View("RoomMaintainance", model); // Trả về view nếu xảy ra lỗi
        }
>>>>>>> Stashed changes

        //[HttpPost]
        //public async Task<IActionResult> ImportRooms(IFormFile excelFile)
        //{
        //    if (excelFile == null || excelFile.Length == 0)
        //    {
        //        ModelState.AddModelError("File", "Vui lòng chọn một file Excel.");
        //        return View();
        //    }

        //    var rooms = new List<Room>();

        //    using (var stream = new MemoryStream())
        //    {
        //        await excelFile.CopyToAsync(stream);
        //        stream.Position = 0;

        //        using (var package = new ExcelPackage(stream))
        //        {
        //            var worksheet = package.Workbook.Worksheets[0]; // Giả sử dữ liệu ở worksheet đầu tiên
        //            int rowCount = worksheet.Dimension.Rows;

        //            for (int row = 2; row <= rowCount; row++) // Bỏ qua dòng đầu vì đó là tiêu đề
        //            {
        //                var room = new Room
        //                {
        //                    RoomNumber = worksheet.Cells[row, 1].Value?.ToString(),
        //                    Area = double.TryParse(worksheet.Cells[row, 2].Value?.ToString(), out double area) ? area : 0,
        //                    Floor = int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out int floor) ? floor : 0,
        //                    Price = decimal.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out decimal price) ? price : 0,
        //                    RoomStatusId = worksheet.Cells[row, 5].Value.ToString() switch
        //                    {
        //                        "Trống" => 1,
        //                        "Đã có người" => 2,
        //                        "Đang sửa chữa" => 3,
        //                        "Sắp trống" => 4,
        //                        _ => 0
        //                    },
        //                    Description = worksheet.Cells[row, 6].Value?.ToString()
        //                };

        //                rooms.Add(room);
        //            }
        //        }
        //    }

        //    // Gửi dữ liệu phòng lên API để lưu vào cơ sở dữ liệu
        //    var json = JsonConvert.SerializeObject(rooms);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");
        //    var response = await client.PostAsync($"{RoomApiUri}/ImportRooms", content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("ListRoom", "Room"); // Chuyển hướng về danh sách phòng sau khi import thành công
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("API", "Lỗi khi lưu dữ liệu vào API.");
        //        return View();
        //    }
        //}



    }
}
