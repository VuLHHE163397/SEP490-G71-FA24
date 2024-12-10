using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RMS_API.Controllers; // Thay bằng namespace thực tế
using RMS_API.Models; // Thay bằng namespace thực tế
using RMS_API.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[TestFixture]
public class UserControllerTests
{
    private RMS_SEP490Context _context;
    private UserController _controller;

    [SetUp]
    public void Setup()
    {
        // Tạo DbContextOptions cho InMemoryDatabase
        var options = new DbContextOptionsBuilder<RMS_SEP490Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Tạo DbContext In-Memory
        _context = new RMS_SEP490Context(options);

        // Xóa sạch dữ liệu trước mỗi test
        _context.Users.RemoveRange(_context.Users);
        _context.SaveChanges();

        // Khởi tạo controller
        _controller = new UserController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose(); // Dọn dẹp DbContext
    }

    [Test]
    public async Task GetAllLanlord_ReturnsNotFound_WhenNoUsersExist()
    {
        // Act
        var result = await _controller.GetAllLanlord();

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.AreEqual("No users found with RoleId: 2", notFoundResult?.Value);
    }

    [Test]
    public async Task GetAllLanlord_ReturnsOk_WhenUsersExist()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            MidName = "M",
            Email = "john.doe@example.com",
            Password = "password",
            Phone = "123456789",
            RoleId = 2,
            UserStatusId = 1,
            UserStatus = new UserStatus { Id = 1, Name = "Active" },
            Role = new Role { Id = 2, Name = "Landlord" }
        });
        _context.SaveChanges();

        // Act
        var result = await _controller.GetAllLanlord();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult?.Value);

        var users = okResult.Value as List<UserDTO>;
        Assert.IsNotNull(users);
        Assert.AreEqual(1, users.Count);
        Assert.AreEqual("John", users[0].FirstName);
    }
}
