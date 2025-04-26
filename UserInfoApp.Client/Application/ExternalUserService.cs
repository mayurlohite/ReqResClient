using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInfoApp.Client.Domains;
using UserInfoApp.Client.Infrastructure;
using UserInfoApp.Client.Models;

namespace UserInfoApp.Client.Application
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IUserApiClient _userApiClient;

        public ExternalUserService(IUserApiClient userApiClient)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            UserEntity userEntity = await _userApiClient.GetUserByIdAsync(userId);
            //We can use automapper as well but it will create dependancy.
            return userEntity != null ? new User()
            {
                Id = userEntity.id,
                FirstName = userEntity.first_name,
                LastName = userEntity.last_name,
                Email = userEntity.email,
                Avatar = userEntity.avatar
            } : null;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var allUsers = new List<UserEntity>();
            var users = new List<User>();
            int currentPage = 1;
            ApiResponseEntity<List<UserEntity>> response;

            do
            {
                response = await _userApiClient.GetUsersAsync(currentPage);
                allUsers.AddRange(response.data);
                currentPage++;
            } while (currentPage <= response.total_pages);

            users = allUsers.Select(userEntity => new User
            {
                Id = userEntity.id,
                FirstName = userEntity.first_name,
                LastName = userEntity.last_name,
                Email = userEntity.email,
                Avatar = userEntity.avatar
            }).ToList();

            return users;
        }
    }
}
