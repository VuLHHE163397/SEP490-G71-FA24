using Castle.Core.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RMS_API.Controllers;
using RMS_API.DTOs;
using RMS_API.Models;
using System.Collections.Generic;
using System.Data;


namespace RMS_UnitTesting
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<RMS_SEP490Context> mockContext;
        private Mock<IConfiguration> mockConfiguration;
        private AuthController authController;



        [SetUp]
        public void Setup()
        {
            mockContext = new Mock<RMS_SEP490Context>();
            mockConfiguration = new Mock<IConfiguration>();
            authController = new AuthController(mockContext.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task ForgotPassword_EmailExists_ReturnsOk()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<User>>();
            var users = new List<User>
        {
            new User { Email = "test@example.com", Password = "oldPassword" }
        }.AsQueryable();

            mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

            // Act
            var result = await authController.ForgotPassword(new AuthController.ForgotPasswordRequest { Email = "test@example.com" }) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Mật khẩu mới đã được gửi qua email của bạn.", result.Value);
        }

        [Test]
        public async Task SendVerificationCode_EmailNotRegistered_ReturnsOk()
        {
            // Arrange
            var userList = new List<User>(); // No users initially
            var dbSetMock = DbSetExtensions.CreateDbSetMock(userList);

            mockContext.Setup(c => c.Users).Returns(dbSetMock.Object);

            var newUser = new RegisterModel
            {
                Email = "new@example.com"
            };

            // Act
            var result = await authController.SendVerificationCode(newUser) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Code đã được gửi qua thư của bạn.", result.Value);
        }


        [Test]
        public async Task Register_ValidCode_ReturnsOk()
        {
            // Arrange
            var email = "valid@example.com";
            var verificationCode = "123456";

            // Inject a valid code into the controller's dictionary
            authController._verificationCodes[email] = verificationCode;

            var registerModel = new RegisterModel
            {
                Email = email,
                Password = "password",
                VerificationCode = verificationCode,
                FirstName = "Test",
                LastName = "User",
                MidName = "Middle",
                Phone = "123456789"
            };

            var userList = new List<User>(); // No users initially
            var dbSetMock = DbSetExtensions.CreateDbSetMock(userList);

            mockContext.Setup(c => c.Users).Returns(dbSetMock.Object);

            // Act
            var result = await authController.Register(registerModel) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Đăng ký thành công.", result.Value);
        }


        [Test]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password"),
                Id = 1,
                Role = new Role { Name = "User" }
            };

            mockContext.Setup(c => c.Users).ReturnsDbSet(new List<User> { user });

            mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("Subjectcode_SoftwareProject490_Group71_Fall2024");
            mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("RMS_API");
            mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("RMS_Client");

            // Act
            var result = await authController.Login(new AuthController.LoginModel
            {
                Email = "test@example.com",
                Password = "password"
            }) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains("token"));
        }
    }
}