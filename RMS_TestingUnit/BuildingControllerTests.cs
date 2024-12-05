using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RMS_API.Controllers;
using RMS_API.DTOs;
using RMS_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace RMS_API.Tests
{
    [TestFixture]
    public class BuildingControllerTests
    {
        private Mock<DbSet<Building>> _mockBuildingSet;
        private Mock<DbSet<Province>> _mockProvinces;
        private Mock<RMS_SEP490Context> _mockContext;
        private BuildingController _controller;

        [SetUp]
        public void Setup()
        {
            _mockBuildingSet = new Mock<DbSet<Building>>();
            _mockProvinces = new Mock<DbSet<Province>>();
            _mockContext = new Mock<RMS_SEP490Context>();
            _controller = new BuildingController(_mockContext.Object);
        }

        // Test for CheckBuildingName
        [Test]
        public void CheckBuildingName_ReturnsConflict_WhenBuildingNameExists()
        {
            var userId = 1;
            var name = "Building A";

            var existingBuildings = new List<Building>
            {
                new Building { Name = "Building A", UserId = 1 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Building>>();
            mockSet.As<IQueryable<Building>>().Setup(m => m.Provider).Returns(existingBuildings.Provider);
            mockSet.As<IQueryable<Building>>().Setup(m => m.Expression).Returns(existingBuildings.Expression);
            mockSet.As<IQueryable<Building>>().Setup(m => m.ElementType).Returns(existingBuildings.ElementType);
            mockSet.As<IQueryable<Building>>().Setup(m => m.GetEnumerator()).Returns(existingBuildings.GetEnumerator());

            _mockContext.Setup(c => c.Buildings).Returns(mockSet.Object);

            var result = _controller.CheckBuildingName(userId, name);

            Assert.IsInstanceOf<ConflictObjectResult>(result);
        }

        // Test for GetDistrictsByProvince - Bad Request when province name is null or empty
        [Test]
        public void GetDistrictsByProvince_ReturnsBadRequest_WhenProvinceNameIsNullOrEmpty()
        {
            // Arrange: Mock the context (we don't care about DbSet for this test)
            var mockContext = new Mock<RMS_SEP490Context>();

            // Create the controller and pass in the mock context
            var controller = new BuildingController(mockContext.Object);

            // Act: Call the GetDistrictsByProvince method with a null or empty province name
            var result = controller.GetDistrictsByProvince(null);

            // Assert: Check that the result is BadRequest with the appropriate message
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Province name cannot be null or empty.", badRequestResult.Value);
        }

        // Test for GetDistrictsByProvince - Not Found when no districts exist for the given province
        [Test]
        public void GetDistrictsByProvince_ReturnsNotFound_WhenNoDistrictsExistForProvince()
        {
            // Arrange: Mock the DbSet for Districts with no matching districts for the given province
            var districts = new List<District>().AsQueryable();

            var mockSet = new Mock<DbSet<District>>();
            mockSet.As<IQueryable<District>>().Setup(m => m.Provider).Returns(districts.Provider);
            mockSet.As<IQueryable<District>>().Setup(m => m.Expression).Returns(districts.Expression);
            mockSet.As<IQueryable<District>>().Setup(m => m.ElementType).Returns(districts.ElementType);
            mockSet.As<IQueryable<District>>().Setup(m => m.GetEnumerator()).Returns(districts.GetEnumerator());

            // Mock RMS_SEP490Context
            var mockContext = new Mock<RMS_SEP490Context>();
            mockContext.Setup(c => c.Districts).Returns(mockSet.Object);

            // Create the controller and pass in the mock context
            var controller = new BuildingController(mockContext.Object);

            // Act: Call the GetDistrictsByProvince method with a province name
            var result = controller.GetDistrictsByProvince("Hà Nội");

            // Assert: Check that the result is NotFound with the appropriate message
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("No districts found for the given province.", notFoundResult.Value);
        }

        // Test for GetAllBuildings - Returns Ok with correct BuildingDTO list
        [Test]
        public async Task GetAllBuildings_ReturnsOkResult_WithBuildingDTOs()
        {
            // Arrange: Mock Buildings
            var buildings = new List<Building>
            {
                new Building
                {
                    Id = 1,
                    Name = "Building A",
                    TotalFloors = 5,
                    NumberOfRooms = 10,
                    Distance = 100,
                    LinkEmbedMap = "http://map.com",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Address = new Address { Information = "123 Street", Ward = new Ward { Name = "Cống Vị" }, District = new District { Name = "Ba Đình" }, Province = new Province { Name = "Hà Nội" } },
                    BuildingStatus = new BuildingStatus { Name = "Active" },
                    User = new User(),
                    Province = new Province { Name = "Hà Nội" },
                    District = new District { Name = "Ba Đình" },
                    Ward = new Ward { Name = "Cống Vị" }
                }
            };

            // Mock DbSet<Building>
            var mockSet = new Mock<DbSet<Building>>();
            mockSet.As<IQueryable<Building>>().Setup(m => m.Provider).Returns(buildings.AsQueryable().Provider);
            mockSet.As<IQueryable<Building>>().Setup(m => m.Expression).Returns(buildings.AsQueryable().Expression);
            mockSet.As<IQueryable<Building>>().Setup(m => m.ElementType).Returns(buildings.AsQueryable().ElementType);
            mockSet.As<IQueryable<Building>>().Setup(m => m.GetEnumerator()).Returns(buildings.AsQueryable().GetEnumerator());

            // Mock DbContext to return the mocked DbSet<Building>
            _mockContext.Setup(c => c.Buildings).Returns(mockSet.Object);

            // Act: Call the GetAllBuildings method
            var result = _controller.GetAllBuildings() as OkObjectResult;// Await the method that returns IActionResult

            // Assert: Check that the result is Ok and contains the correct data
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var buildingDtos = okResult.Value as List<BuildingDTO>;
            Assert.IsNotNull(buildingDtos);
            Assert.AreEqual(1, buildingDtos.Count);  // Ensure there is one building in the result

            // Validate the fields of the BuildingDTO
            var buildingDto = buildingDtos[0];
            Assert.AreEqual(1, buildingDto.Id);
            Assert.AreEqual("Building A", buildingDto.Name);
            Assert.AreEqual(5, buildingDto.TotalFloors);
            Assert.AreEqual(10, buildingDto.NumberOfRooms);
            Assert.AreEqual(100, buildingDto.Distance);
            Assert.AreEqual("http://map.com", buildingDto.LinkEmbedMap);
            Assert.IsNotNull(buildingDto.CreatedDate);
            Assert.IsNotNull(buildingDto.UpdatedDate);
            Assert.AreEqual("Hà Nội", buildingDto.ProvinceName);
            Assert.AreEqual("Ba Đình", buildingDto.DistrictName);
            Assert.AreEqual("Cống Vị", buildingDto.WardName);
            Assert.AreEqual("123 Street,Cống Vị, Ba Đình, Hà Nội", buildingDto.AddressDetails);
            Assert.AreEqual("Active", buildingDto.BuildingStatus);
        }


        [Test]
        public async Task GetAllBuildings_ReturnsOkResult_EmptyList_WhenNoBuildingsExist()
        {
            // Arrange: Mock an empty list of buildings
            var buildings = new List<Building>();

            // Mock DbSet<Building>
            var mockSet = new Mock<DbSet<Building>>();
            mockSet.As<IQueryable<Building>>().Setup(m => m.Provider).Returns(buildings.AsQueryable().Provider);
            mockSet.As<IQueryable<Building>>().Setup(m => m.Expression).Returns(buildings.AsQueryable().Expression);
            mockSet.As<IQueryable<Building>>().Setup(m => m.ElementType).Returns(buildings.AsQueryable().ElementType);
            mockSet.As<IQueryable<Building>>().Setup(m => m.GetEnumerator()).Returns(buildings.AsQueryable().GetEnumerator());

            // Mock DbContext to return the mocked empty DbSet<Building>
            _mockContext.Setup(c => c.Buildings).Returns(mockSet.Object);

            // Act: Call the GetAllBuildings method
            var result = _controller.GetAllBuildings() as OkObjectResult; // Await the async method call

            // Assert: Check that the result is Ok and contains an empty list
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var buildingDtos = okResult.Value as List<BuildingDTO>;
            Assert.IsNotNull(buildingDtos);
            Assert.AreEqual(0, buildingDtos.Count);  // Ensure the result is an empty list

            // You can also check that the returned list is empty by asserting the count is 0
        }

       


        // Test when no provinces are available
        [Test]
        public async Task GetProvinces_ReturnsNotFound_WhenNoProvincesExist()
        {
            // Arrange: Mock an empty list of provinces
            var provinces = new List<Province>();

            // Mock DbSet<Province>
            _mockProvinces.As<IQueryable<Province>>().Setup(m => m.Provider).Returns(provinces.AsQueryable().Provider);
            _mockProvinces.As<IQueryable<Province>>().Setup(m => m.Expression).Returns(provinces.AsQueryable().Expression);
            _mockProvinces.As<IQueryable<Province>>().Setup(m => m.ElementType).Returns(provinces.AsQueryable().ElementType);
            _mockProvinces.As<IQueryable<Province>>().Setup(m => m.GetEnumerator()).Returns(provinces.AsQueryable().GetEnumerator());

            // Mock DbContext to return the mocked DbSet<Province>
            _mockContext.Setup(c => c.Provinces).Returns(_mockProvinces.Object);

            // Act: Call the GetProvinces method
            var result =  _controller.GetProvinces() as NotFoundObjectResult;

            // Assert: Check that the result is NotFound
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("No provinces found.", result.Value);
        }

        [Test]
        public async Task GetBuildingInformationById_ReturnsNotFound_WhenBuildingDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Không thêm dữ liệu vào context => giả lập không có dữ liệu
            var controller = new BuildingController(context);

            // Act
            var result = await controller.GetBuildinImformationgById(1) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result); // Kết quả không null
            Assert.AreEqual(404, result.StatusCode); // Mã trạng thái phải là 404
            Assert.AreEqual("Building not found.", result.Value); // Kiểm tra thông điệp trả về
        }


        [Test]
        public async Task GetBuildingInformationById_ReturnsOk_WhenBuildingExists()
        {
            // Arrange: Cấu hình cơ sở dữ liệu InMemory
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Thêm một building vào cơ sở dữ liệu
            var building = new Building
            {
                Id = 1,
                Name = "Test Building",
                TotalFloors = 5,
                Distance = 10,
                NumberOfRooms = 10,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Address = new Address { Information = "Test Address" },
                BuildingStatus = new BuildingStatus { Name = "Active" },
                Province = new Province { Name = "Test Province" },
                District = new District { Name = "Test District" },
                Ward = new Ward { Name = "Test Ward" },
                LinkEmbedMap = "http://testmap.com"
            };
            context.Buildings.Add(building);
            await context.SaveChangesAsync(); // Lưu thay đổi vào InMemoryDatabase

            var controller = new BuildingController(context);

            // Act: Gọi API với ID building
            var result = await controller.GetBuildinImformationgById(1) as OkObjectResult;

            // Assert: Kiểm tra kết quả trả về
            Assert.IsNotNull(result); // Kết quả không null
            Assert.AreEqual(200, result.StatusCode); // Mã trạng thái phải là 200
            Assert.IsInstanceOf<BuildingDTO>(result.Value); // Kiểm tra kiểu dữ liệu trả về
            var returnedBuilding = result.Value as BuildingDTO;
            Assert.AreEqual(building.Id, returnedBuilding.Id); // Đảm bảo dữ liệu trả về đúng
            Assert.AreEqual(building.Name, returnedBuilding.Name);
            Assert.AreEqual(building.TotalFloors, returnedBuilding.TotalFloors);
            Assert.AreEqual(building.Distance, returnedBuilding.Distance);
            Assert.AreEqual(building.NumberOfRooms, returnedBuilding.NumberOfRooms);
            Assert.AreEqual(building.Province.Name, returnedBuilding.ProvinceName);
            Assert.AreEqual(building.District.Name, returnedBuilding.DistrictName);
            Assert.AreEqual(building.Ward.Name, returnedBuilding.WardName);
            Assert.AreEqual(building.Address.Information, returnedBuilding.AddressDetails);
            Assert.AreEqual(building.BuildingStatus.Name, returnedBuilding.BuildingStatus);
            Assert.AreEqual(building.LinkEmbedMap, returnedBuilding.LinkEmbedMap);
        }

        [Test]
        public async Task GetBuildingById_ReturnsOk_WhenBuildingExists()
        {
            // Arrange: Cấu hình cơ sở dữ liệu InMemory
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Thêm một building vào cơ sở dữ liệu
            var building = new Building
            {
                Id = 1,
                Name = "Test Building",
                TotalFloors = 5,
                Distance = 10,
                NumberOfRooms = 10,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Address = new Address { Information = "Test Address" },
                BuildingStatus = new BuildingStatus { Name = "Active" },
                Province = new Province { Name = "Test Province" },
                District = new District { Name = "Test District" },
                Ward = new Ward { Name = "Test Ward" },
                LinkEmbedMap = "http://testmap.com"
            };
            context.Buildings.Add(building);
            await context.SaveChangesAsync(); // Lưu thay đổi vào InMemoryDatabase

            var controller = new BuildingController(context);

            // Act: Gọi API với ID building
            var result = await controller.GetBuildingById(1) as OkObjectResult;

            // Assert: Kiểm tra kết quả trả về
            Assert.IsNotNull(result); // Kết quả không null
            Assert.AreEqual(200, result.StatusCode); // Mã trạng thái phải là 200
            Assert.IsInstanceOf<BuildingDTO>(result.Value); // Kiểm tra kiểu dữ liệu trả về
            var returnedBuilding = result.Value as BuildingDTO;
            Assert.AreEqual(building.Id, returnedBuilding.Id); // Đảm bảo dữ liệu trả về đúng
            Assert.AreEqual(building.Name, returnedBuilding.Name);
            Assert.AreEqual(building.TotalFloors, returnedBuilding.TotalFloors);
            Assert.AreEqual(building.Distance, returnedBuilding.Distance);
            Assert.AreEqual(building.NumberOfRooms, returnedBuilding.NumberOfRooms);
            Assert.AreEqual(building.Province.Name, returnedBuilding.ProvinceName);
            Assert.AreEqual(building.District.Name, returnedBuilding.DistrictName);
            Assert.AreEqual(building.Ward.Name, returnedBuilding.WardName);
            Assert.AreEqual(building.Address.Information, returnedBuilding.AddressDetails);
            Assert.AreEqual(building.BuildingStatus.Name, returnedBuilding.BuildingStatus);
            Assert.AreEqual(building.LinkEmbedMap, returnedBuilding.LinkEmbedMap);
        }

        [Test]
        public async Task GetBuildingById_ReturnsNotFound_WhenBuildingDoesNotExist()
        {
            // Arrange: Cấu hình cơ sở dữ liệu InMemory
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            var controller = new BuildingController(context);

            // Act: Gọi API với ID building không tồn tại
            var result = await controller.GetBuildingById(999) as NotFoundObjectResult;

            // Assert: Kiểm tra mã trạng thái trả về là 404
            Assert.IsNotNull(result); // Kết quả không null
            Assert.AreEqual(404, result.StatusCode); // Mã trạng thái phải là 404 (NotFound)
            Assert.AreEqual("Building not found.", result.Value); // Kiểm tra thông báo lỗi
        }

        [Test]
        public async Task GetBuildingStatus_ReturnsOk_WhenStatusesExist()
        {
            // Arrange: Cấu hình cơ sở dữ liệu InMemory
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Thêm một vài BuildingStatus vào cơ sở dữ liệu
            var buildingStatus1 = new BuildingStatus { Id = 1, Name = "Active" };
            var buildingStatus2 = new BuildingStatus { Id = 2, Name = "Inactive" };
            context.BuildingStatuses.Add(buildingStatus1);
            context.BuildingStatuses.Add(buildingStatus2);
            await context.SaveChangesAsync(); // Lưu thay đổi vào InMemoryDatabase

            var controller = new BuildingController(context);

            // Act: Gọi API GetBuildingStatus
            var result = await controller.GetBuildingStaus() as OkObjectResult;

            // Assert: Kiểm tra kết quả trả về
            Assert.IsNotNull(result); // Kết quả không null
            Assert.AreEqual(200, result.StatusCode); // Mã trạng thái phải là 200
            Assert.IsInstanceOf<List<BuildingStatus>>(result.Value); // Kiểm tra kiểu dữ liệu trả về
            var returnedStatuses = result.Value as List<BuildingStatus>;
            Assert.AreEqual(2, returnedStatuses.Count); // Kiểm tra số lượng item trả về
            Assert.AreEqual(buildingStatus1.Name, returnedStatuses[0].Name); // Kiểm tra tên trạng thái đầu tiên
            Assert.AreEqual(buildingStatus2.Name, returnedStatuses[1].Name); // Kiểm tra tên trạng thái thứ hai
        }

        [Test]
        public async Task GetBuildingStatus_ReturnsOk_WhenNoStatusesExist()
        {
            // Arrange: Cấu hình cơ sở dữ liệu InMemory
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            var controller = new BuildingController(context);

            // Act: Gọi API GetBuildingStatus khi không có status nào
            var result = await controller.GetBuildingStaus() as OkObjectResult;

            // Assert: Kiểm tra kết quả trả về
            Assert.IsNotNull(result); // Kết quả không null
            Assert.AreEqual(200, result.StatusCode); // Mã trạng thái phải là 200
            Assert.IsInstanceOf<List<BuildingStatus>>(result.Value); // Kiểm tra kiểu dữ liệu trả về
            var returnedStatuses = result.Value as List<BuildingStatus>;
            Assert.AreEqual(0, returnedStatuses.Count); // Kiểm tra không có status nào
        }

      /*  [Test]
        public async Task EditBuildingById_ReturnsOk_WhenBuildingAndRelatedDataExist()
        {
            // Arrange: Set up the in-memory database
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Create a new database for each test
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Add necessary data to the in-memory database
            var province = new Province { Id = 1, Name = "Test Province" };
            var district = new District { Id = 1, Name = "Test District", ProvincesId = province.Id };
            var ward = new Ward { Id = 1, Name = "Test Ward", DistrictId = district.Id };
            var buildingStatus = new BuildingStatus { Id = 1, Name = "Test Status" };

            context.Provinces.Add(province);
            context.Districts.Add(district);
            context.Wards.Add(ward);
            context.BuildingStatuses.Add(buildingStatus);
            await context.SaveChangesAsync();

            // Add a building to the in-memory database
            var building = new Building
            {
                Id = 1,
                Name = "Existing Building",
                Distance = 100,
                TotalFloors = 10,
                NumberOfRooms = 50,
                ProvinceId = province.Id,
                DistrictId = district.Id,
                WardId = ward.Id,
                BuildingStatusId = buildingStatus.Id,
                LinkEmbedMap = "https://example.com/map",
                UpdatedDate = DateTime.UtcNow
            };

            context.Buildings.Add(building);
            await context.SaveChangesAsync();

            var controller = new BuildingController(context);

            // Create a BuildingDTO with the updated information
            var buildingDto = new BuildingDTO
            {
                Id = 1, // Existing building ID
                Name = "Updated Building",
                Distance = 200,
                TotalFloors = 20,
                NumberOfRooms = 100,
                ProvinceName = province.Name,
                DistrictName = district.Name,
                WardName = ward.Name,
                BuildingStatus = buildingStatus.Name,
                LinkEmbedMap = "https://example.com/updated-map",
                AddressDetails = "Updated Address"
            };

            // Act: Call the EditBuildingById API with the existing building ID and updated data
            var result = await controller.EditBuildingById(1, buildingDto);

            // Assert: Check the returned result
            var okResult = result.Result as OkObjectResult; // Convert from ActionResult<BuildingDTO> to OkObjectResult

            Assert.IsNotNull(okResult); // Ensure the result is not null
            Assert.AreEqual(200, okResult.StatusCode); // The status code should be 200 (OK)

            var updatedBuildingDto = okResult.Value as BuildingDTO;
            Assert.IsNotNull(updatedBuildingDto); // Ensure the value is not null
            Assert.AreEqual(buildingDto.Name, updatedBuildingDto.Name); // Check that the building name is updated
            Assert.AreEqual(buildingDto.Distance, updatedBuildingDto.Distance); // Check that the distance is updated
            Assert.AreEqual(buildingDto.TotalFloors, updatedBuildingDto.TotalFloors); // Check total floors
            Assert.AreEqual(buildingDto.NumberOfRooms, updatedBuildingDto.NumberOfRooms); // Check number of rooms
            Assert.AreEqual(buildingDto.LinkEmbedMap, updatedBuildingDto.LinkEmbedMap); // Check the map link
        }*/


        [Test]
        public async Task EditBuildingById_ReturnsNotFound_WhenBuildingNotFound()
        {
            // Arrange: Cấu hình cơ sở dữ liệu InMemory
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Tạo database mới cho mỗi test
                .Options;

            using var context = new RMS_SEP490Context(options);

            var controller = new BuildingController(context);

            // Tạo BuildingDTO mới với dữ liệu sẽ được cập nhật
            var buildingDto = new BuildingDTO
            {
                Id = 999, // ID không tồn tại trong cơ sở dữ liệu
                Name = "Non-Existing Building"
            };

            // Act: Gọi API EditBuildingById với ID không tồn tại
            var result = await controller.EditBuildingById(999, buildingDto);

            // Assert: Kiểm tra kết quả trả về
            var notFoundResult = result.Result as NotFoundObjectResult;  // Chuyển từ ActionResult<BuildingDTO> thành NotFoundObjectResult

            Assert.IsNotNull(notFoundResult); // Kết quả không null
            Assert.AreEqual(404, notFoundResult.StatusCode); // Mã trạng thái phải là 404
            Assert.AreEqual("Building not found.", notFoundResult.Value); // Kiểm tra thông điệp lỗi
        }


        /*  [Test]
          public async Task GetWardsByDistrict_ReturnsOk_WhenDistrictExists()
          {
              // Arrange: Set up the in-memory database
              var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                  .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Create a new database for each test
                  .Options;

              using var context = new RMS_SEP490Context(options);

              // Add necessary data to the in-memory database
              var district = new District { Id = 1, Name = "Test District" };
              var ward1 = new Ward { Id = 1, Name = "Test Ward 1", DistrictId = 1 };
              var ward2 = new Ward { Id = 2, Name = "Test Ward 2", DistrictId = 1 };

              context.Districts.Add(district);
              context.Wards.Add(ward1);
              context.Wards.Add(ward2);
              await context.SaveChangesAsync();

              var controller = new BuildingController(context);

              // Act: Call the GetWardsByDistrict API with a valid district name
              Task<IActionResult> resultTask = controller.GetWardsByDistrict("Test District"); // Store the task result

              // Wait for the task to complete
              var result = await resultTask; // Await the task to get the IActionResult

              // Assert: Check the returned result
              var okResult = result as OkObjectResult;  // Convert from IActionResult to OkObjectResult

              Assert.IsNotNull(okResult); // Ensure the result is not null
              Assert.AreEqual(200, okResult.StatusCode); // The status code should be 200 (OK)

              var wards = okResult.Value as List<dynamic>;
              Assert.IsNotNull(wards); // Ensure that wards are returned
              Assert.AreEqual(2, wards.Count); // There should be 2 wards for the "Test District"
              Assert.AreEqual("Test Ward 1", wards[0].Name); // Check the name of the first ward
              Assert.AreEqual("Test Ward 2", wards[1].Name); // Check the name of the second ward
          }*/

        [Test]
        public async Task GetBuildingsByUserId_ReturnsOk_WhenBuildingsExist()
        {
            // Arrange: Create an in-memory database
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Create a User with required properties
            var user = new User
            {
                Id = 1,
                Email = "testuser@example.com",  // Email is required
                FirstName = "John",              // FirstName is required
                LastName = "Doe",                // LastName is required
                MidName = "Middle",              // MidName is required
                Password = "password",           // Password is required
                Phone = "1234567890"             // Phone is required
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Create a Building and associate it with the user
            var building = new Building
            {
                Name = "Test Building",
                UserId = user.Id,
                TotalFloors = 5,
                NumberOfRooms = 10,
                Distance = 50.0,
                LinkEmbedMap = "https://example.com",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Address = new Address
                {
                    Information = "123 Street",
                    Ward = new Ward { Name = "Test Ward" },
                    District = new District { Name = "Test District" },
                    Province = new Province { Name = "Test Province" }
                },
                BuildingStatus = new BuildingStatus { Name = "Active" },
                Province = new Province { Name = "Test Province" },
                District = new District { Name = "Test District" },
                Ward = new Ward { Name = "Test Ward" }
            };

            // Add the building to the context
            context.Buildings.Add(building);
            await context.SaveChangesAsync();

            // Create a BuildingController instance
            var controller = new BuildingController(context);

            // Act: Call GetBuildingsByUserId with the userId
            var result = await controller.GetBuildingsByUserId(user.Id);

            // Assert: Verify that the result is an Ok response
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var buildings = okResult.Value as List<BuildingDTO>;
            Assert.IsNotNull(buildings);
            Assert.AreEqual(1, buildings.Count);  // Verify that one building is returned
        }


        [Test]
        public async Task GetBuildingsByUserId_ReturnsNotFound_WhenNoBuildingsExist()
        {
            // Arrange: Create an in-memory database
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Create a User with required properties
            var user = new User
            {
                Id = 1,
                Email = "testuser@example.com",  // Email is required
                FirstName = "John",              // FirstName is required
                LastName = "Doe",                // LastName is required
                MidName = "Middle",              // MidName is required
                Password = "password",           // Password is required
                Phone = "1234567890"             // Phone is required
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Create a BuildingController instance
            var controller = new BuildingController(context);

            // Act: Call GetBuildingsByUserId with a user that has no buildings
            var result = await controller.GetBuildingsByUserId(user.Id);

            // Assert: Verify that the result is a NotFound response
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual($"No buildings found for user with ID {user.Id}.", notFoundResult.Value);
        }

        [Test]
        public void GetWardsByDistrict_ReturnsBadRequest_WhenDistrictNameIsNullOrEmpty()
        {
            // Arrange: Mock the context (we don't care about DbSet for this test)
            var mockContext = new Mock<RMS_SEP490Context>();

            // Create the controller and pass in the mock context
            var controller = new BuildingController(mockContext.Object);

            // Act: Call the GetWardsByDistrict method with a null or empty district name
            var result = controller.GetWardsByDistrict(null);

            // Assert: Check that the result is BadRequest with the appropriate message
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("District name cannot be null or empty.", badRequestResult.Value);
        }
        [Test]
        public async Task GetWardsByDistrict_SimplifiedQuery_ReturnsExpectedData()
        {
            // Arrange: Create an in-memory database
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Create a District and Wards
            var district = new District { Name = "Test District" };
            var ward1 = new Ward { Name = "Test Ward 1", District = district };
            var ward2 = new Ward { Name = "Test Ward 2", District = district };

            // Add the data to context
            context.Districts.Add(district);
            context.Wards.AddRange(ward1, ward2);
            await context.SaveChangesAsync();

            // Directly query the data (bypass controller for debugging)
            var wards = await context.Wards
                .Where(w => w.District.Name == "Test District")
                .Select(w => new { w.Id, w.Name })
                .ToListAsync();

            // Assert that the wards are found
            Assert.IsNotNull(wards, "Wards should not be null.");
            Assert.AreEqual(2, wards.Count, "Expected 2 wards.");
            Assert.AreEqual("Test Ward 1", wards[0].Name);
            Assert.AreEqual("Test Ward 2", wards[1].Name);
        }

        [Test]
        public async Task AddBuildingbyId_DuplicateBuildingName_ReturnsConflictResult()
        {
            // Arrange: Create an in-memory database
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Create mock data for user, province, district, ward, and building status
            var user = new User
            {
                Id = 1,
                FirstName = "Test",    // Set FirstName
                LastName = "User",     // Set LastName
                MidName = "Middle",    // Set MidName
                Email = "testuser@example.com", // Add required properties
                Password = "Password123",
                Phone = "1234567890",
                UserStatusId = 1, // Assign a valid UserStatusId
                RoleId = 1        // Assign a valid RoleId
            };

            var province = new Province { Id = 1, Name = "Test Province" };
            var district = new District { Id = 1, Name = "Test District", ProvincesId = province.Id };
            var ward = new Ward { Id = 1, Name = "Test Ward", DistrictId = district.Id };
            var buildingStatus = new BuildingStatus { Id = 1, Name = "Active" };
            var existingBuilding = new Building
            {
                Name = "Test Building",
                UserId = 1,
                ProvinceId = province.Id,
                DistrictId = district.Id,
                WardId = ward.Id,
                BuildingStatusId = buildingStatus.Id
            };

            // Add existing data to the context
            context.Users.Add(user);
            context.Provinces.Add(province);
            context.Districts.Add(district);
            context.Wards.Add(ward);
            context.BuildingStatuses.Add(buildingStatus);
            context.Buildings.Add(existingBuilding);
            await context.SaveChangesAsync();

            // Create DTO for a new building with the same name
            var buildingDto = new AddBuildingDTO
            {
                UserId = 1,
                Name = "Test Building",  // Duplicate name
                TotalFloors = 10,
                NumberOfRooms = 100,
                Distance = 5.0,
                AddressDetails = "123 Test Street",
                ProvinceName = "Test Province",
                DistrictName = "Test District",
                WardName = "Test Ward",
                BuildingStatus = "Active",
                LinkEmbedMap = "https://www.example.com"
            };

            var controller = new BuildingController(context);

            // Act: Call the AddBuildingbyId method
            var result = await controller.AddBuildingbyId(buildingDto);

            // Assert: Verify that a Conflict result is returned
            Assert.IsInstanceOf<ConflictObjectResult>(result);
            var conflictResult = result as ConflictObjectResult;
            Assert.AreEqual("Tên tòa nhà trùng với tên hiện có. Vui lòng nhập lại.", conflictResult.Value);
        }

        [Test]
        public async Task AddBuildingbyId_InvalidProvince_ReturnsBadRequest()
        {
            // Arrange: Create an in-memory database
            var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RMS_SEP490Context(options);

            // Create mock data for user, district, ward, and building status
            var user = new User
            {
                Id = 1,
                FirstName = "Test",    // Set FirstName
                LastName = "User",     // Set LastName
                MidName = "Middle",    // Set MidName
                Email = "testuser@example.com", // Add required properties
                Password = "Password123",
                Phone = "1234567890",
                UserStatusId = 1, // Assign a valid UserStatusId
                RoleId = 1        // Assign a valid RoleId
            };

            var district = new District { Id = 1, Name = "Test District" };
            var ward = new Ward { Id = 1, Name = "Test Ward", DistrictId = district.Id };
            var buildingStatus = new BuildingStatus { Id = 1, Name = "Active" };

            // Add data to the context
            context.Users.Add(user);
            context.Districts.Add(district);
            context.Wards.Add(ward);
            context.BuildingStatuses.Add(buildingStatus);
            await context.SaveChangesAsync();

            // Create DTO with a non-existent province
            var buildingDto = new AddBuildingDTO
            {
                UserId = 1,
                Name = "New Building",
                TotalFloors = 10,
                NumberOfRooms = 100,
                Distance = 5.0,
                AddressDetails = "123 Test Street",
                ProvinceName = "NonExistentProvince",  // Invalid province
                DistrictName = "Test District",
                WardName = "Test Ward",
                BuildingStatus = "Active",
                LinkEmbedMap = "https://www.example.com"
            };

            var controller = new BuildingController(context);

            // Act: Call the AddBuildingbyId method
            var result = await controller.AddBuildingbyId(buildingDto);

            // Assert: Verify that a BadRequest result is returned
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Province 'NonExistentProvince' not found.", badRequestResult.Value);
        }

    }
}
