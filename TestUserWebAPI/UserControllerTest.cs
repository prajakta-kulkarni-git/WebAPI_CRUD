using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserWebAPI.Controllers;
using UserWebAPI.Data;
using UserWebAPI.Model;
using Xunit;

namespace TestUserWebAPI
{
    public class UserControllerTest
    {
        [Fact]
        public async Task GetUsers_ReturnsOkResult()
        {
            // Arrange
            var users = new List<User>
    {
        new User { Id = Guid.NewGuid(), FirstName = "Prajakta", LastName = "Kulkarni", Email = "Prajakta@example.com" },
        new User { Id = Guid.NewGuid(), FirstName = "Sush", LastName = "Kulkarni", Email = "Sush@example.com" }
    };

            using (var context = CreateTestDbContext())
            {
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            using (var context = CreateTestDbContext())
            {
                var controller = CreateTestController(context);

                // Act
                var result = await controller.GetUsers();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
                Assert.Equal(users.Count, returnedUsers.Count());
            }
        }

        [Fact]
        public async Task GetUsersByEmail_ExistingUser_ReturnsOkResult()
        {
            // Arrange
            var email = "test@example.com";

            // Create a test in-memory database context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestUserDb") // Use the same database name as in Program.cs
                .Options;

            var dbContext = new ApplicationDbContext(options); // Create a real instance of the in-memory database context

            // Seed the in-memory database with test data
            dbContext.Users.Add(new User { Email = email });
            dbContext.SaveChanges();

            var mockLogger = new Mock<ILogger<UserController>>();
            var controller = new UserController(dbContext, mockLogger.Object);

            // Act
            var result = await controller.GetUsers(email);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUsersByEmail_NonExistingUser_ReturnsNotFoundResult()
        {
            // Arrange
            var email = "nonexistent@example.com";

            var dbContext = CreateTestDbContext(); // Create a real instance of the in-memory database context
            var controller = CreateTestController(dbContext);
            //Act
            var result = await controller.GetUsers(email);
            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_InvalidUser_ReturnsBadRequestResult()
        {
            // Arrange: Create an invalid user request (e.g., missing required fields)
            var invalidUserRequest = new AddUserRequest { };

            var mockDbContext = new Mock<ApplicationDbContext>();
            var mockLogger = new Mock<ILogger<UserController>>();
            var controller = new UserController(mockDbContext.Object, mockLogger.Object);

            // Act
            var result = await controller.Create(invalidUserRequest);

            // Assert          
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Please enter at least email Id.", conflictResult.Value);
        }

        [Fact]
        public async Task Create_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var addUserRequest = new AddUserRequest
            {
                FirstName = "Prajakta",
                LastName = "Kulkarni",
                Email = "prajakta@example.com",
                Address = "123 Main St",
                ContactNumber = 1234567890
            };

            using (var dbContext = CreateTestDbContext())
            {
                var controller = CreateTestController(dbContext);

                // Act
                var result = await controller.Create(addUserRequest);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var createdUser = Assert.IsType<User>(okResult.Value);

                // Add additional assertions here to check if the user was created correctly.
                Assert.Equal(addUserRequest.FirstName, createdUser.FirstName);
                Assert.Equal(addUserRequest.LastName, createdUser.LastName);
                Assert.Equal(addUserRequest.Email, createdUser.Email);
            }
        }

        [Fact]
        public async Task Create_DuplicateEmail_ReturnsConflictResult()
        {
            // Arrange
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Existing",
                LastName = "User",
                Email = "prajakta@example.com"
            };

            var addUserRequest = new AddUserRequest
            {
                FirstName = "Prajakta",
                LastName = "Kulkarni",
                Email = "prajakta@example.com",
                Address = "123 Main St",
                ContactNumber = 1234567890
            };

            using (var dbContext = CreateTestDbContext())
            {
                // Seed the database with an existing user
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var controller = CreateTestController(dbContext);

                // Act
                var result = await controller.Create(addUserRequest);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal($"User with email '{addUserRequest.Email}' already exists.", conflictResult.Value);
            }
        }

        [Fact]
        public async Task Create_NullEmail_ReturnsConflictResult()
        {
            // Arrange
            var addUserRequest = new AddUserRequest
            {
                FirstName = "Prajakta",
                LastName = "Kulkarni",
                Email = null, // Null email
                Address = "123 Main St",
                ContactNumber = 1234567890
            };

            using (var dbContext = CreateTestDbContext())
            {
                var controller = CreateTestController(dbContext);

                // Act
                var result = await controller.Create(addUserRequest);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal("Please enter at least email Id.", conflictResult.Value);
            }
        }

        [Fact]
        public async Task UpdateByEmail_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var updateUserRequest = new UpdateUserRequest
            {
                Email = "prajakta@example.com", // Provide an existing email
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Address = "Updated Address",
                ContactNumber = 9876543210
            };

            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Prajakta",
                LastName = "Kulkarni",
                Email = "prajakta@example.com",
                Address = "123 Main St",
                ContactNumber = 1234567890
            };

            using (var dbContext = CreateTestDbContext())
            {
                dbContext.Users.Add(existingUser);
                dbContext.SaveChanges();

                var controller = CreateTestController(dbContext);

                // Act
                var result = await controller.UpdateByEmail(updateUserRequest);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var updatedUser = Assert.IsType<User>(okResult.Value);

                Assert.Equal(updateUserRequest.FirstName, updatedUser.FirstName);
                Assert.Equal(updateUserRequest.LastName, updatedUser.LastName);
                Assert.Equal(updateUserRequest.Address, updatedUser.Address);
                Assert.Equal(updateUserRequest.ContactNumber, updatedUser.ContactNumber);
            }
        }

        [Fact]
        public async Task UpdateByEmail_NonExistingUser_ReturnsNotFoundResult()
        {
            // Arrange
            var updateUserRequest = new UpdateUserRequest
            {
                Email = "nonexistent@example.com", // Provide a non-existing email
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Address = "Updated Address",
                ContactNumber = 9876543210
            };

            using (var dbContext = CreateTestDbContext())
            {
                var controller = CreateTestController(dbContext);

                // Act
                var result = await controller.UpdateByEmail(updateUserRequest);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal($"User with email '{updateUserRequest.Email}' not found.", notFoundResult.Value);
            }
        }
        public ApplicationDbContext CreateTestDbContext()
        {
            // Ensure the in-memory database is created
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }
        public UserController CreateTestController(ApplicationDbContext dbContext)
        {
            var mockLogger = new Mock<ILogger<UserController>>();
            return new UserController(dbContext, mockLogger.Object);
        }

    }
}

