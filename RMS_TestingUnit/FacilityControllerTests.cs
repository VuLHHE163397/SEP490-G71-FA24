using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS_API.Controllers; // Replace with your actual namespace
using RMS_API.Models;     // Replace with your actual namespace
using RMS_API.DTOs;       // Replace with your actual namespace
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[TestFixture]
public class FacilityControllerTests
{
    private RMS_SEP490Context _context;
    private FacilityController _controller;

    [SetUp]
    public void Setup()
    {
        // Configure DbContextOptions for InMemoryDatabase
        var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Create In-Memory DbContext
        _context = new RMS_SEP490Context(options);

        // Clear existing data before each test
        _context.Facilities.RemoveRange(_context.Facilities);
        _context.FacilitiesStatuses.RemoveRange(_context.FacilitiesStatuses);
        _context.SaveChanges();

        // Initialize the controller
        _controller = new FacilityController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose(); // Clean up the DbContext
    }

    [Test]
    public async Task CreateAsync_ReturnsBadRequest_WhenStatusIdIsInvalid()
    {
        // Arrange
        var facilityDTO = new FacilityDTO
        {
            Name = "Test Facility",
            RoomId = 1,
            statusId = null, // Invalid statusId
            UserId = 1
        };

        // Act
        var result = await _controller.CreateAsync(facilityDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Facility status '' not found.", badRequestResult?.Value);
    }

    [Test]
    public async Task CreateAsync_ReturnsOk_WhenFacilityIsCreatedSuccessfully()
    {
        // Arrange
        var facilityStatus = new FacilitiesStatus
        {
            Id = 1,
            Description = "Available",
            Status = 1
        };
        _context.FacilitiesStatuses.Add(facilityStatus);
        _context.SaveChanges();

        var facilityDTO = new FacilityDTO
        {
            Name = "Test Facility",
            RoomId = 1,
            statusId = 1,
            UserId = 1
        };

        // Act
        var result = await _controller.CreateAsync(facilityDTO);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult?.Value);

        var createdFacility = okResult.Value as Facility;
        Assert.IsNotNull(createdFacility);
        Assert.AreEqual("Test Facility", createdFacility.Name);
        Assert.AreEqual(1, createdFacility.RoomId);
        Assert.AreEqual(1, createdFacility.FacilityStatusId);
        Assert.AreEqual(1, createdFacility.UserId);
    }

    [Test]
    public async Task CreateAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        var facilityDTO = new FacilityDTO
        {
            Name = "Test Facility",
            RoomId = 1,
            statusId = 1,
            UserId = 1
        };

        // Simulate an exception by passing a null DbContext
        _controller = new FacilityController(null);

        // Act
        var result = await _controller.CreateAsync(facilityDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult?.Value);
        Assert.IsTrue(badRequestResult.Value.ToString().Contains("Object reference"));
    }


    [Test]
    public async Task UpdateAsync_ReturnsBadRequest_WhenFacilityDoesNotExist()
    {
        // Arrange
        var facilityDTO = new FacilityDTO
        {
            Id = 999, // Non-existent facility ID
            Name = "Updated Facility",
            RoomId = 2,
            statusId = 1,
            UserId = 2
        };

        // Act
        var result = await _controller.UpdateAsync(facilityDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Facility not found", badRequestResult?.Value);
    }

    [Test]
    public async Task UpdateAsync_ReturnsOk_WhenFacilityIsUpdatedSuccessfully()
    {
        // Arrange
        var facilityStatus = new FacilitiesStatus
        {
            Id = 1,
            Description = "Available",
            Status = 1
        };
        _context.FacilitiesStatuses.Add(facilityStatus);

        var facility = new Facility
        {
            Id = 1,
            Name = "Test Facility",
            RoomId = 1,
            FacilityStatusId = 1,
            UserId = 1
        };
        _context.Facilities.Add(facility);
        _context.SaveChanges();

        var facilityDTO = new FacilityDTO
        {
            Id = 1,
            Name = "Updated Facility",
            RoomId = 2,
            statusId = 1,
            UserId = 1
        };

        // Act
        var result = await _controller.UpdateAsync(facilityDTO);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult?.Value);

        var updatedFacility = okResult.Value as Facility;
        Assert.IsNotNull(updatedFacility);
        Assert.AreEqual("Updated Facility", updatedFacility.Name);
        Assert.AreEqual(2, updatedFacility.RoomId);
        Assert.AreEqual(1, updatedFacility.FacilityStatusId);
    }

    [Test]
    public async Task UpdateAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        var facilityDTO = new FacilityDTO
        {
            Id = 1,
            Name = "Updated Facility",
            RoomId = 2,
            statusId = 1,
            UserId = 1
        };

        // Simulate an exception by passing a null DbContext
        _controller = new FacilityController(null);

        // Act
        var result = await _controller.UpdateAsync(facilityDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult?.Value);
        Assert.IsTrue(badRequestResult.Value.ToString().Contains("Object reference"));
    }


    [Test]
    public async Task DeleteAsync_ReturnsBadRequest_WhenFacilityDoesNotExist()
    {
        // Arrange
        int invalidId = 999; // ID that does not exist

        // Act
        var result = await _controller.DeleteAsync(invalidId);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Facility not found", badRequestResult?.Value);
    }

    [Test]
    public async Task DeleteAsync_ReturnsOk_WhenFacilityIsDeletedSuccessfully()
    {
        // Arrange
        var facility = new Facility
        {
            Id = 1,
            Name = "Test Facility",
            RoomId = 1,
            FacilityStatusId = 1,
            UserId = 2
        };
        _context.Facilities.Add(facility);
        _context.SaveChanges();

        // Act
        var result = await _controller.DeleteAsync(facility.Id);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.AreEqual(true, okResult?.Value);

        // Verify the facility was removed from the database
        var deletedFacility = await _context.Facilities.FirstOrDefaultAsync(f => f.Id == facility.Id);
        Assert.IsNull(deletedFacility);
    }

    [Test]
    public async Task DeleteAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        int facilityId = 1;

        // Simulate an exception by passing a null DbContext
        _controller = new FacilityController(null);

        // Act
        var result = await _controller.DeleteAsync(facilityId);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult?.Value);
        Assert.IsTrue(badRequestResult.Value.ToString().Contains("Object reference"));
    }

    

    [Test]
    public async Task GetByRoomId_ReturnsOk_WithEmptyList_WhenNoMatchingFacilities()
    {
        // Arrange
        var room = new Room
        {
            Id = 1,
            RoomNumber = "A101",
            Description = "Test Room Description" // Add the required Description property
        };
        _context.Rooms.Add(room);
        _context.SaveChanges();

        // Act
        var result = await _controller.GetByRoomId(2, 123); // Non-existent RoomId or UserId

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult?.Value);

        var facilityTableView = okResult.Value as FacilityTableView;
        Assert.IsNotNull(facilityTableView);
        Assert.AreEqual(0, facilityTableView.Total);
        Assert.IsEmpty(facilityTableView.Facilities);
    }

    [Test]
    public async Task GetByRoomId_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Simulate an exception by passing null DbContext
        _controller = new FacilityController(null);

        // Act
        var result = await _controller.GetByRoomId(1, 123);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult?.Value);
        Assert.IsTrue(badRequestResult.Value.ToString().Contains("Object reference"));
    }

}
