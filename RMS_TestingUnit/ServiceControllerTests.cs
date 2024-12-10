using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS_API.Controllers;
using RMS_API.DTOs;
using RMS_API.Models;

[TestFixture]
public class ServiceControllerTests
{
    private RMS_SEP490Context _context;
    private ServiceController _controller;

    [SetUp]
    public void Setup()
    {
        // Create DbContextOptions for InMemoryDatabase
        var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Create In-Memory DbContext
        _context = new RMS_SEP490Context(options);

        // Clear existing data before each test
        _context.Services.RemoveRange(_context.Services);
        _context.Buildings.RemoveRange(_context.Buildings); // Make sure all related data is cleared
        _context.SaveChanges();

        // Initialize controller
        _controller = new ServiceController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose(); // Clean up DbContext
    }

    [Test]
    public async Task CreateAsync_ReturnsBadRequest_WhenBuildingIdIsNull()
    {
        // Arrange
        var serviceDTO = new ServiceDTO
        {
            Name = "Test Service",
            Price = 100,
            UserId = 1,
            BuildingId = null, // Missing BuildingId
            type = 1
        };

        // Act
        var result = await _controller.CreateAsync(serviceDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Building not found", badRequestResult?.Value);
    }

    [Test]
    public async Task CreateAsync_ReturnsOk_WhenServiceIsCreatedSuccessfully()
    {
        // Arrange
        var building = new Building
        {
            Id = 1,
            Name = "Building A"
        };
        _context.Buildings.Add(building);
        _context.SaveChanges();

        var serviceDTO = new ServiceDTO
        {
            Name = "Test Service",
            Price = 100,
            UserId = 1,
            BuildingId = building.Id, // Valid BuildingId
            type = 1
        };

        // Act
        var result = await _controller.CreateAsync(serviceDTO);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult?.Value);

        var createdService = okResult.Value as Service;
        Assert.IsNotNull(createdService);
        Assert.AreEqual("Test Service", createdService.Name);
        Assert.AreEqual(100, createdService.Price);
        Assert.AreEqual(1, createdService.UserId);
        Assert.AreEqual(building.Id, createdService.BuildingId);
    }

    [Test]
    public async Task CreateAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        var serviceDTO = new ServiceDTO
        {
            Name = "Test Service",
            Price = 100,
            UserId = 1,
            BuildingId = 1, // Assuming BuildingId doesn't exist in the in-memory context
            type = 1
        };

        // Simulate an exception
        _controller = new ServiceController(null);

        // Act
        var result = await _controller.CreateAsync(serviceDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult?.Value);
        Assert.IsTrue(badRequestResult.Value.ToString().Contains("Object reference"));
    }


    [Test]
    public async Task UpdateAsync_ReturnsOk_WhenServiceIsUpdatedSuccessfully()
    {
        // Arrange
        var building = new Building
        {
            Id = 1,
            Name = "Building A"
        };
        _context.Buildings.Add(building);
        _context.SaveChanges();

        var service = new Service
        {
            Name = "Old Service",
            Price = 100,
            UserId = 1,
            BuildingId = building.Id,
            Type = 1
        };
        _context.Services.Add(service);
        _context.SaveChanges();

        var serviceDTO = new ServiceDTO
        {
            Id = service.Id, // Same Id as the service to update
            Name = "Updated Service",
            Price = 150,
            UserId = 1,
            BuildingId = building.Id,
            type = 1
        };

        // Act
        var result = await _controller.UpdateAsync(serviceDTO);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult?.Value);

        var updatedService = okResult.Value as Service;
        Assert.IsNotNull(updatedService);
        Assert.AreEqual("Updated Service", updatedService.Name);
        Assert.AreEqual(150, updatedService.Price);
    }

    [Test]
    public async Task UpdateAsync_ReturnsBadRequest_WhenServiceNotFound()
    {
        // Arrange
        var serviceDTO = new ServiceDTO
        {
            Id = 999, // Non-existent ID
            Name = "Non-Existent Service",
            Price = 100,
            UserId = 1,
            BuildingId = 1,
            type = 1
        };

        // Act
        var result = await _controller.UpdateAsync(serviceDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.AreEqual("Service not found", badRequestResult?.Value);
    }

    [Test]
    public async Task UpdateAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        var serviceDTO = new ServiceDTO
        {
            Id = 1, // Assuming the service doesn't exist in the in-memory context
            Name = "Test Service",
            Price = 100,
            UserId = 1,
            BuildingId = 1,
            type = 1
        };

        // Simulate an exception
        _controller = new ServiceController(null); // Null context to simulate an exception

        // Act
        var result = await _controller.UpdateAsync(serviceDTO);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult?.Value);
        Assert.IsTrue(badRequestResult.Value.ToString().Contains("Object reference"));
    }

    [Test]
    public async Task DeleteAsync_ReturnsOk_WhenServiceIsDeletedSuccessfully()
    {
        // Arrange
        var serviceId = 1;
        var service = new Service { Id = serviceId, Name = "Test Service", Price = 100 };

        _context.Services.Add(service); // Add to in-memory database
        await _context.SaveChangesAsync();

        var controller = new ServiceController(_context);

        // Act
        var result = await controller.DeleteAsync(serviceId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(true, okResult.Value);
    }

    [Test]
    public async Task DeleteAsync_ReturnsBadRequest_WhenServiceNotFound()
    {
        // Arrange
        var nonExistentServiceId = 99;
        var controller = new ServiceController(_context);

        // Act
        var result = await controller.DeleteAsync(nonExistentServiceId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Service not found", badRequestResult.Value);
    }

    [Test]
    public async Task DeleteAsync_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        var serviceId = 1;
        _context.Services.Add(new Service { Id = serviceId, Name = "Test Service", Price = 100 });
        await _context.SaveChangesAsync();

        // Simulate an error by making the database unavailable or adding a faulty mock
        _context.Services = null; // Forces a null reference exception

        var controller = new ServiceController(_context);

        // Act
        var result = await controller.DeleteAsync(serviceId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }
    [Test]
    public async Task DeleteAsync_RemovesCorrectService_WhenMultipleServicesExist()
    {
        // Arrange
        var service1 = new Service { Id = 1, Name = "Service 1", Price = 50 };
        var service2 = new Service { Id = 2, Name = "Service 2", Price = 100 };

        _context.Services.AddRange(service1, service2);
        await _context.SaveChangesAsync();

        var controller = new ServiceController(_context);

        // Act
        var result = await controller.DeleteAsync(service1.Id);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        // Check if service1 is deleted and service2 remains
        var remainingService = await _context.Services.FindAsync(service2.Id);
        Assert.NotNull(remainingService);
        var deletedService = await _context.Services.FindAsync(service1.Id);
        Assert.Null(deletedService); // Ensure service1 was deleted
    }


}
