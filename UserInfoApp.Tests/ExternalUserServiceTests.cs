using Moq;
using UserInfoApp.Client.Application;
using UserInfoApp.Client.Domains;
using UserInfoApp.Client.Exceptions;
using UserInfoApp.Client.Infrastructure;
using UserInfoApp.Client.Models;

namespace UserInfoApp.Tests
{
    public class ExternalUserServiceTests
    {
        private readonly Mock<IUserApiClient> _apiClientMock;
        private readonly ExternalUserService _service;

        public ExternalUserServiceTests()
        {
            _apiClientMock = new Mock<IUserApiClient>();
            _service = new ExternalUserService(_apiClientMock.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenApiCallSucceeds()
        {
            // Arrange
            var userEntity = new UserEntity { id =1,  first_name = "George",  last_name = "Bluth",  email= "george.bluth@reqres.in", avatar= "https://reqres.in/img/faces/1-image.jpg" };
            _apiClientMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(userEntity);

            User? user = userEntity != null ? new User()
            {
                Id = userEntity.id,
                FirstName = userEntity.first_name,
                LastName = userEntity.last_name,
                Email = userEntity.email,
                Avatar = userEntity.avatar
            } : null;


            // Act
            var result = await _service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("george.bluth@reqres.in", result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsApiException_WhenApiCallFails()
        {
            // Arrange
            var expectedMessage = "User not found";
            var expectedStatusCode = 404;

            _apiClientMock
                .Setup(x => x.GetUserByIdAsync(1))
                .ThrowsAsync(new UserInfoAppException(expectedMessage, expectedStatusCode));

            // Act
            try
            {
                await _service.GetUserByIdAsync(1);
                Assert.True(false, "Expected exception was not thrown.");
            }
            catch (UserInfoAppException ex)
            {
                // Assert
                Assert.Equal(expectedMessage, ex.Message);
                Assert.Equal(expectedStatusCode, ex.StatusCode);
            }
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers_WhenPaginatedApiCallSucceeds()
        {
            // Arrange

           

            var usersPage1 = new List<UserEntity> { new UserEntity { id = 1 }, new UserEntity { id = 2 }, new UserEntity { id = 3 }, new UserEntity { id = 4 }, new UserEntity { id = 5 }, new UserEntity { id = 6 } };
            var usersPage2 = new List<UserEntity> { new UserEntity { id = 7 }, new UserEntity { id = 8 }, new UserEntity { id = 9 }, new UserEntity { id = 10 }, new UserEntity { id = 11 }, new UserEntity { id = 12 } };
            _apiClientMock.Setup(x => x.GetUsersAsync(1))
                .ReturnsAsync(new ApiResponseEntity<List<UserEntity>> { data = usersPage1, total_pages = 2 });
            _apiClientMock.Setup(x => x.GetUsersAsync(2))
                .ReturnsAsync(new ApiResponseEntity<List<UserEntity>> { data = usersPage2, total_pages = 2 });

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.Equal(12, result.Count());
        }
    }
}
