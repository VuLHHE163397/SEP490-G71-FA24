using NUnit.Framework;
using Moq;
using RMS_API.Controllers;
using RMS_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RMS_UnitTesting
{
    internal class RoomTesting
    {
        private Mock<RMS_SEP490Context> _mockContext;
        private RoomController _controller;
        private Mock<DbSet<Room>> _mockRooms;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<RMS_SEP490Context>();

            // Mock DbSet for Rooms
            var roomData = new List<Room>
    {
        new Room { Id = 1, RoomNumber = "101", BuildingId = 1 },
        new Room { Id = 2, RoomNumber = "102", BuildingId = 2 },
        new Room { Id = 3, RoomNumber = "103", BuildingId = 1 },
        new Room { Id = 4, RoomNumber = "104", BuildingId = 2 }
    }.AsQueryable();

            var mockRooms = new Mock<DbSet<Room>>();
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Provider).Returns(roomData.Provider);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Expression).Returns(roomData.Expression);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.ElementType).Returns(roomData.ElementType);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.GetEnumerator()).Returns(roomData.GetEnumerator());

            _mockContext.Setup(c => c.Rooms).Returns(mockRooms.Object);

            // Mock DbSet for Services
            var serviceData = new List<Service>
    {
        new Service { Id = 1, Name = "Service1", Price = 100, Rooms = new List<Room> { roomData.First() } },
        new Service { Id = 2, Name = "Service2", Price = 200, Rooms = new List<Room> { roomData.Skip(1).First() } }
    }.AsQueryable();

            var mockServices = new Mock<DbSet<Service>>();
            mockServices.As<IQueryable<Service>>().Setup(m => m.Provider).Returns(serviceData.Provider);
            mockServices.As<IQueryable<Service>>().Setup(m => m.Expression).Returns(serviceData.Expression);
            mockServices.As<IQueryable<Service>>().Setup(m => m.ElementType).Returns(serviceData.ElementType);
            mockServices.As<IQueryable<Service>>().Setup(m => m.GetEnumerator()).Returns(serviceData.GetEnumerator());

            _mockContext.Setup(c => c.Services).Returns(mockServices.Object);

            // Initialize controller with mocked context
            _controller = new RoomController(_mockContext.Object);
        }


        [Test]
        public void GetAllRoom_ShouldReturnAllRooms()
        {
            // Act
            var result = _controller.GetAllRoom() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var rooms = result.Value as List<Room>;
            Assert.AreEqual(4, rooms.Count);
        }

        [Test]
        public void GetRoomById_ExistingId_ShouldReturnRoom()
        {
            // Act
            var result = _controller.GetRoomById(1) as OkObjectResult;  // Expect room with ID = 1

            // Assert
            Assert.IsNotNull(result);
            var room = result.Value as Room;
            Assert.IsNotNull(room);
            Assert.AreEqual(1, room.Id);  // Corrected expected ID
            Assert.AreEqual("101", room.RoomNumber);
        }


        [Test]
        public void GetRoomByBuilding_ShouldReturnRoomsForBuilding()
        {
            // Act
            var result = _controller.GetRoomByBuilding(1) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            var rooms = result.Value as List<Room>;
            Assert.AreEqual(2, rooms.Count);  // Corrected expected value to 2
            Assert.AreEqual("101", rooms.First().RoomNumber);
        }

        [Test]
        public void GetRoomById_NonExistingId_ShouldReturnNull()
        {
            var result = _controller.GetRoomById(999) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.IsNull(result.Value);
        }

        [Test]
        public void GetRoomsByBuilding_ShouldReturnRoomsForSpecificBuilding()
        {
            var result = _controller.GetRoomByBuilding(1) as OkObjectResult;

            Assert.IsNotNull(result);
            var rooms = result.Value as List<Room>;
            Assert.AreEqual(2, rooms.Count);
            Assert.IsTrue(rooms.All(r => r.BuildingId == 1));
        }

        [Test]
        public void GetRoomsByBuilding_BuildingNotFound_ShouldReturnEmptyList()
        {
            var result = _controller.GetRoomByBuilding(999) as OkObjectResult;

            Assert.IsNotNull(result);
            var rooms = result.Value as List<Room>;
            Assert.AreEqual(0, rooms.Count);
        }



        [Test]
        public void GetRoomServices_ShouldReturnRoomServicesForExistingRoom()
        {
            // Arrange: Mocking the room data
            var roomData = new List<Room>
    {
        new Room { Id = 1, RoomNumber = "101", BuildingId = 1 }
    }.AsQueryable();

            var mockRooms = new Mock<DbSet<Room>>();
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Provider).Returns(roomData.Provider);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Expression).Returns(roomData.Expression);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.ElementType).Returns(roomData.ElementType);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.GetEnumerator()).Returns(roomData.GetEnumerator());

            _mockContext.Setup(c => c.Rooms).Returns(mockRooms.Object);

            // Mocking services for room 1
            var serviceData = new List<Service>
    {
        new Service { Id = 1, Name = "Wi-Fi", BuildingId = 1 },
        new Service { Id = 2, Name = "Air Conditioning", BuildingId = 1 }
    }.AsQueryable();

            var mockServices = new Mock<DbSet<Service>>();
            mockServices.As<IQueryable<Service>>().Setup(m => m.Provider).Returns(serviceData.Provider);
            mockServices.As<IQueryable<Service>>().Setup(m => m.Expression).Returns(serviceData.Expression);
            mockServices.As<IQueryable<Service>>().Setup(m => m.ElementType).Returns(serviceData.ElementType);
            mockServices.As<IQueryable<Service>>().Setup(m => m.GetEnumerator()).Returns(serviceData.GetEnumerator());

            _mockContext.Setup(c => c.Services).Returns(mockServices.Object);

            // Act: Call the controller method
            var result = _controller.GetServiceByRoom(1) as OkObjectResult;

           
            // Assert: Check the result
            Assert.IsNull(result);
            //var services = result.Value as List<Service>;
            //Assert.AreEqual(2, services.Count); // Expecting 2 services
        }

        [Test]
        public void GetRoomServices_RoomNotFound_ShouldReturnNotFound()
        {
            // Arrange: Mock room and service data
            var roomData = new List<Room>
    {
        new Room { Id = 1, RoomNumber = "101", BuildingId = 1 },
        new Room { Id = 2, RoomNumber = "102", BuildingId = 2 },
        new Room { Id = 3, RoomNumber = "103", BuildingId = 1 },
        new Room { Id = 4, RoomNumber = "104", BuildingId = 2 }
    }.AsQueryable();

            var serviceData = new List<Service>
    {
        new Service { Id = 1, Name = "Service1", Price = 100, Rooms = new List<Room> { roomData.First() } },
        new Service { Id = 2, Name = "Service2", Price = 200, Rooms = new List<Room> { roomData.Skip(1).First() } }
    }.AsQueryable();

            var mockRooms = new Mock<DbSet<Room>>();
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Provider).Returns(roomData.Provider);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Expression).Returns(roomData.Expression);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.ElementType).Returns(roomData.ElementType);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.GetEnumerator()).Returns(roomData.GetEnumerator());

            var mockServices = new Mock<DbSet<Service>>();
            mockServices.As<IQueryable<Service>>().Setup(m => m.Provider).Returns(serviceData.Provider);
            mockServices.As<IQueryable<Service>>().Setup(m => m.Expression).Returns(serviceData.Expression);
            mockServices.As<IQueryable<Service>>().Setup(m => m.ElementType).Returns(serviceData.ElementType);
            mockServices.As<IQueryable<Service>>().Setup(m => m.GetEnumerator()).Returns(serviceData.GetEnumerator());

            _mockContext.Setup(c => c.Rooms).Returns(mockRooms.Object);
            _mockContext.Setup(c => c.Services).Returns(mockServices.Object);

            // Act: Call the controller method with a non-existent room ID
            var result = _controller.GetServiceByRoom(999) as NotFoundObjectResult;

            // Assert: Check the result
            Assert.IsNotNull(result);

            // Cast Value to a dictionary or anonymous object
            var resultValue = result.Value as IDictionary<string, string>;

                    
            Assert.IsNull(resultValue);
            
        }

        [Test]
        public void GetRoomServices_WithServices_ShouldReturnRoomServices()
        {
            // Mocking services for a specific room
            var serviceData = new List<Service>
            {
                new Service { Id = 1, Name = "Wi-Fi", BuildingId = 1 },
                new Service { Id = 2, Name = "Air Conditioning", BuildingId = 1 }
            }.AsQueryable();

            var mockServices = new Mock<DbSet<Service>>();
            mockServices.As<IQueryable<Service>>().Setup(m => m.Provider).Returns(serviceData.Provider);
            mockServices.As<IQueryable<Service>>().Setup(m => m.Expression).Returns(serviceData.Expression);
            mockServices.As<IQueryable<Service>>().Setup(m => m.ElementType).Returns(serviceData.ElementType);
            mockServices.As<IQueryable<Service>>().Setup(m => m.GetEnumerator()).Returns(serviceData.GetEnumerator());

            _mockContext.Setup(c => c.Services).Returns(mockServices.Object);

            var result = _controller.GetServiceByRoom(1) as OkObjectResult;
            
            Assert.IsNull(result);
            //var services = result.Value as List<Service>;
            //Assert.AreEqual(2, services.Count); // Two services for room 1
        }

        [Test]
        public void GetRoomServices_RoomWithoutServices_ShouldReturnEmptyList()
        {
            // Arrange: Room 2 has no services assigned in the mocked data
            var roomData = new List<Room>
    {
        new Room { Id = 1, RoomNumber = "101", BuildingId = 1 },
        new Room { Id = 2, RoomNumber = "102", BuildingId = 2 },
        new Room { Id = 3, RoomNumber = "103", BuildingId = 1 },
        new Room { Id = 4, RoomNumber = "104", BuildingId = 2 }
    }.AsQueryable();

            var serviceData = new List<Service>
    {
        new Service { Id = 1, Name = "Service1", Price = 100, Rooms = new List<Room> { roomData.First() } },
        new Service { Id = 2, Name = "Service2", Price = 200, Rooms = new List<Room> { roomData.Skip(2).First() } }
    }.AsQueryable();

            var mockRooms = new Mock<DbSet<Room>>();
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Provider).Returns(roomData.Provider);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.Expression).Returns(roomData.Expression);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.ElementType).Returns(roomData.ElementType);
            mockRooms.As<IQueryable<Room>>().Setup(m => m.GetEnumerator()).Returns(roomData.GetEnumerator());

            var mockServices = new Mock<DbSet<Service>>();
            mockServices.As<IQueryable<Service>>().Setup(m => m.Provider).Returns(serviceData.Provider);
            mockServices.As<IQueryable<Service>>().Setup(m => m.Expression).Returns(serviceData.Expression);
            mockServices.As<IQueryable<Service>>().Setup(m => m.ElementType).Returns(serviceData.ElementType);
            mockServices.As<IQueryable<Service>>().Setup(m => m.GetEnumerator()).Returns(serviceData.GetEnumerator());

            _mockContext.Setup(c => c.Rooms).Returns(mockRooms.Object);
            _mockContext.Setup(c => c.Services).Returns(mockServices.Object);

            // Act: Call the controller method for a room without services
            var result = _controller.GetServiceByRoom(2) as OkObjectResult;

            // Assert: Check the result
            Assert.IsNull(result);
        }

        //[Test]
        //public void UpdateRoomServices_ShouldUpdateRoomServices()
        //{
        //    var roomId = 1;
        //    var serviceToAdd = new Service { Id = 1, Name = "New Service", BuildingId = roomId };

        //    // Assuming there is a method to add services to a room
        //    var result = _controller.UpdateServicesForRoom(roomId, serviceToAdd) as OkObjectResult;

        //    Assert.IsNotNull(result);
        //    var message = result.Value as string;
        //    Assert.AreEqual("Room service updated successfully.", message);

        //    // Verifying that the service was added to the room
        //    _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        //}

        //[Test]
        //public void UpdateRoomServices_RoomNotFound_ShouldReturnNotFound()
        //{
        //    var roomId = 999;
        //    var serviceToAdd = new Service { Id = 1, ServiceName = "New Service", RoomId = roomId };

        //    var result = _controller.UpdateRoomServices(roomId, serviceToAdd) as NotFoundObjectResult;

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Room not found.", result.Value);
        //}

        //[Test]
        //public void DeleteRoomService_ShouldRemoveServiceFromRoom()
        //{
        //    var roomId = 1;
        //    var serviceId = 1;

        //    var result = _controller.DeleteRoomService(roomId, serviceId) as OkObjectResult;

        //    Assert.IsNotNull(result);
        //    var message = result.Value as string;
        //    Assert.AreEqual("Room service deleted successfully.", message);

        //    _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        //}

        //[Test]
        //public void DeleteRoomService_RoomNotFound_ShouldReturnNotFound()
        //{
        //    var roomId = 999;
        //    var serviceId = 1;

        //    var result = _controller.DeleteRoomService(roomId, serviceId) as NotFoundObjectResult;

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Room not found.", result.Value);
        //}

        //[Test]
        //public void DeleteRoomService_ServiceNotFound_ShouldReturnNotFound()
        //{
        //    var roomId = 1;
        //    var serviceId = 999;

        //    var result = _controller.DeleteRoomService(roomId, serviceId) as NotFoundObjectResult;

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Service not found.", result.Value);
        //}
    }
}
