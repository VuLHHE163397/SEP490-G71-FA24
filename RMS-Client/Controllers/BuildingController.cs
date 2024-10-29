using Microsoft.AspNetCore.Mvc;
using RMS_Client.Services; // Đảm bảo sử dụng namespace đúng


namespace RMS_Client.Controllers
{
    public class BuildingController : Controller
    {
        private readonly BuildingService _buildingService;

        public BuildingController(BuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        // Action để lấy danh sách các tòa nhà
        public async Task<IActionResult> ListBuilding()
        {
            var buildings = await _buildingService.GetAllBuildingsAsync(); 
            return View(buildings); // Trả về view với danh sách tòa nhà
        }
    }
}
