using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInfoApp.Client.Domains;

namespace UserInfoApp.Client.Infrastructure
{
    public interface IUserApiClient
    {
        Task<UserEntity> GetUserByIdAsync(int userId);
        Task<ApiResponseEntity<List<UserEntity>>> GetUsersAsync(int pageNumber);
    }
}
