using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using RMS_API.Controllers;
using RMS_API.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using RMS_API.DTOs;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using static RMS_API.Controllers.AuthController;

namespace RMS_UnitTesting
{
    public class AuthTesting
    {
        private AuthController _authController;
        private Mock<RMS_SEP490Context> _mockContext;
        private Mock<IConfiguration> _mockConfig;


        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<RMS_SEP490Context>();
            _mockConfig = new Mock<IConfiguration>();
            _authController = new AuthController(_mockContext.Object, _mockConfig.Object);
        }

        [Test]
        public async Task ForgotPassword_EmailIsEmpty_ReturnsBadRequest()
        {
            // Arrange
            var request = new AuthController.ForgotPasswordRequest { Email = "" };

            // Act
            var result = await _authController.ForgotPassword(request);

            // Assert
            Assert.IsInstanceOf<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
            var badRequest = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.AreEqual("Vui lòng nhập email.", badRequest.Value);
        }

        private Mock<DbSet<T>> GetMockedDbSet<T>(List<T> data) where T : class
        {
            var queryableData = data.AsQueryable();

            var mockedDbSet = new Mock<DbSet<T>>();
            mockedDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockedDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockedDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockedDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());


            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x["Jwt:Key"]).Returns("Subjectcode_SoftwareProject490_Group71_Fall2024");
            mockConfiguration.SetupGet(x => x["Jwt:Issuer"]).Returns("RMS_API");
            mockConfiguration.SetupGet(x => x["Jwt:Audience"]).Returns("RMS_Client");
            // Support async operations
            mockedDbSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));

            mockedDbSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));

            return mockedDbSet;
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken) =>
                new(_inner.MoveNext());

            public ValueTask<bool> MoveNextAsync()
            {
                throw new NotImplementedException();
            }

            public T Current => _inner.Current;
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression) =>
                new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
                new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression) =>
                _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) =>
                _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })
                    ?.MakeGenericMethod(resultType)
                    .Invoke(_inner, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))?
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult });
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        [Test]
        public async Task ForgotPassword_EmailNotFound_ReturnsBadRequest()
        {
            // Arrange
            var mockUsers = GetMockedDbSet(new List<User>()); // Empty list to simulate "not found"
            _mockContext.Setup(ctx => ctx.Users).Returns(mockUsers.Object);

            var request = new AuthController.ForgotPasswordRequest { Email = "notfound@test.com" };

            // Act
            var result = await _authController.ForgotPassword(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result); // Only checks type
            var badRequest = result as BadRequestObjectResult; // Safely cast after type check
            Assert.NotNull(badRequest); // Ensure casting was successful
            Assert.AreEqual("Email không tồn tại.", badRequest.Value);
        }

        [Test]
        public async Task ForgotPassword_ValidEmail_SendsEmail()
        {
            // Arrange
            var users = new List<User>
    {
        new User { Email = "found@test.com", Password = "oldpassword" }
    };
            var mockUsers = GetMockedDbSet(users); // Mock the DbSet
            _mockContext.Setup(ctx => ctx.Users).Returns(mockUsers.Object); // Mock Users DbSet

            _mockContext.Setup(ctx => ctx.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _authController.ForgotPassword(new AuthController.ForgotPasswordRequest { Email = "found@test.com" });

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result); // Assert type
            var okResult = result as OkObjectResult; // Cast after assertion
            Assert.NotNull(okResult); // Ensure casting was successful
            Assert.AreEqual("Mật khẩu mới đã được gửi qua email của bạn.", okResult.Value);
        }


        [Test]
        public async Task SendVerificationCode_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var users = new List<User>
    {
        new User { Email = "duplicate@test.com" }
    };
            var mockUsers = GetMockedDbSet(users); // Mock the DbSet
            _mockContext.Setup(ctx => ctx.Users).Returns(mockUsers.Object); // Setup mocked Users DbSet

            // Act
            var result = await _authController.SendVerificationCode(new RegisterModel { Email = "duplicate@test.com" });

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result); // Assert type
            var badRequest = result as BadRequestObjectResult; // Cast after assertion
            Assert.NotNull(badRequest); // Ensure casting was successful
            Assert.AreEqual("Email này đã được đăng ký!", badRequest.Value);
        }

        [Test]
        public async Task Register_InvalidVerificationCode_ReturnsBadRequest()
        {
            // Arrange
            _authController.GetType()
                .GetField("_verificationCodes", BindingFlags.NonPublic | BindingFlags.Static)!
                .SetValue(null, new Dictionary<string, string> { { "user@test.com", "123456" } });

            var registerModel = new RegisterModel { Email = "user@test.com", VerificationCode = "654321" };

            // Act
            var result = await _authController.Register(registerModel);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result); // Only checks type
            var badRequest = result as BadRequestObjectResult; // Safely cast after type check
            Assert.NotNull(badRequest); // Ensure casting was successful
            Assert.AreEqual("Code lỗi hoặc đã hết hạn. Mã code phải có 6 số.", badRequest.Value);
        }

        //[Test]
        //public async Task Register_ValidVerificationCode_Success()
        //{
        //    // Arrange
        //    _authController.GetType()
        //        .GetField("_verificationCodes", BindingFlags.NonPublic | BindingFlags.Static)!
        //        .SetValue(null, new Dictionary<string, string> { { "user@test.com", "123456" } });

        //    var registerModel = new RegisterModel { Email = "user@test.com", VerificationCode = "123456" };

        //    _mockContext.Setup(ctx => ctx.Users.AddAsync(It.IsAny<User>(), default))
        //.ReturnsAsync((User user, CancellationToken _) =>
        //    new ValueTask<EntityEntry<User>>(Mock.Of<EntityEntry<User>>()));
        //    _mockContext.Setup(ctx => ctx.SaveChangesAsync(default)).ReturnsAsync(1);

        //    // Act
        //    var result = await _authController.Register(registerModel);

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result); // Assert type
        //    var okResult = result as OkObjectResult; // Cast after assertion
        //    Assert.NotNull(okResult); // Ensure casting was successful
        //    Assert.AreEqual("Đăng ký thành công.", okResult.Value);
        //}

        [Test]
        public async Task SendVerificationCode_ValidEmail_SendsCode()
        {
            // Arrange
            var users = new List<User>(); // Empty list simulates no user in the database
            var mockUsersDbSet = GetMockedDbSet(users);  // Get the mocked DbSet
            _mockContext.Setup(ctx => ctx.Users).Returns(mockUsersDbSet.Object);

            var validRegisterModel = new RegisterModel { Email = "valid@test.com" };

            // Act
            var result = await _authController.SendVerificationCode(validRegisterModel);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result); // Assert type
            var okResult = result as OkObjectResult; // Cast after assertion
            Assert.NotNull(okResult); // Ensure casting was successful
            Assert.AreEqual("Code đã được gửi qua thư của bạn.", okResult.Value); // Verify response message
        }



        [Test]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            var users = new List<User>(); // Simulate no users
            var mockUsers = GetMockedDbSet(users);
            _mockContext.Setup(ctx => ctx.Users).Returns(mockUsers.Object);

            var loginModel = new LoginModel { Email = "notfound@test.com", Password = "password123" };

            // Act
            var result = await _authController.Login(loginModel);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.AreEqual("Tài khoản không tồn tại. Xin hãy đăng nhập lại!", unauthorizedResult.Value); // Updated message
        }

        //[Test]
        //public async Task Login_ValidCredentials_ReturnsToken()
        //{
        //    // Arrange
        //    var user = new User
        //    {
        //        Email = "user@test.com",
        //        Password = BCrypt.Net.BCrypt.HashPassword("123456789"),
        //        Role = new Role { Name = "Admin" }               
        //    };

        //    // Mock the DbSet<User> and setup SingleOrDefaultAsync to return the user
        //    var mockDbSet = new Mock<DbSet<User>>();
        //    mockDbSet.As<IQueryable<User>>()
        //             .Setup(m => m.Provider)
        //             .Returns(new TestAsyncQueryProvider<User>(new List<User> { user }.AsQueryable().Provider));
        //    mockDbSet.As<IQueryable<User>>()
        //             .Setup(m => m.Expression)
        //             .Returns(new List<User> { user }.AsQueryable().Expression);
        //    mockDbSet.As<IQueryable<User>>()
        //             .Setup(m => m.ElementType)
        //             .Returns(new List<User> { user }.AsQueryable().ElementType);
        //    mockDbSet.As<IQueryable<User>>()
        //             .Setup(m => m.GetEnumerator())
        //             .Returns(new List<User> { user }.GetEnumerator());
        //    _mockContext.Setup(ctx => ctx.Users).Returns(mockDbSet.Object);

        //    // Mock the configuration
        //    var mockConfiguration = new Mock<IConfiguration>();
        //    mockConfiguration.SetupGet(x => x["Jwt:Key"]).Returns("Subjectcode_SoftwareProject490_Group71_Fall2024");
        //    mockConfiguration.SetupGet(x => x["Jwt:Issuer"]).Returns("RMS_API");
        //    mockConfiguration.SetupGet(x => x["Jwt:Audience"]).Returns("RMS_Client");

        //    // Create the controller with the mocked configuration and context
        //    var controller = new AuthController( _mockContext.Object, mockConfiguration.Object);

        //    // Act
        //    var result = await controller.Login(new AuthController.LoginModel
        //    {
        //        Email = "user@test.com",
        //        Password = "123456789"
        //    });

        //    // Assert
        //    Assert.IsInstanceOf<OkObjectResult>(result); // Assert type
        //    var okResult = result as OkObjectResult; // Cast after assertion
        //    Assert.NotNull(okResult); // Ensure casting was successful
        //    Assert.NotNull(okResult.Value); // Ensure a valid response
        //}
    }
}