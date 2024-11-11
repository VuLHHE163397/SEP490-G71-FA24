﻿using Microsoft.AspNetCore.Http;
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

namespace RMS_Client.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient client = null;
        private string RoomApiUri = "https://localhost:7056/api/Room";
        private string BuildingApiUri = "https://localhost:7056/api/Building";

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
            return View(room);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRoom()
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
                    return RedirectToAction("ListRoom", new { buildingId = buildingId });
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
