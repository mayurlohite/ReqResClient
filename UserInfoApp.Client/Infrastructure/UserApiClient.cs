using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UserInfoApp.Client.Domains;
using UserInfoApp.Client.Exceptions;
using UserInfoApp.Client.Settings;

namespace UserInfoApp.Client.Infrastructure
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration;

        public UserApiClient(HttpClient httpClient, IMemoryCache memoryCache, IOptions<ApiConfigs> apiConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            //Set cache duration
            int minutes = apiConfig?.Value?.CacheDurationMinutes ?? 5;
            _cacheDuration = TimeSpan.FromMinutes(minutes);
        }

        public async Task<UserEntity> GetUserByIdAsync(int userId)
        {
            string cacheKey = $"user_{userId}";
            if (_cache.TryGetValue(cacheKey, out UserEntity cachedUser))
            {
                return cachedUser;
            }
            try
            {
                //Send request to API
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/users/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new UserInfoAppException($"Failed to fetch user {userId}. Status: {response.StatusCode}", (int)response.StatusCode);
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseEntity<UserEntity>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                //If NULL
                if (apiResponse?.data == null)
                    throw new UserInfoAppException($"Deserialized data is null for user {userId}", 500);

                //If Not Null - Set Cache
                _cache.Set(cacheKey, apiResponse.data, _cacheDuration);
                return apiResponse.data;
            }
            catch (HttpRequestException ex)
            {
                throw new UserInfoAppException("Network error occurred while fetching user.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new UserInfoAppException("The request timed out while fetching user.", ex);
            }
            catch (JsonException ex)
            {
                throw new UserInfoAppException("Failed to deserialize user data.", ex);
            }
        }

        public async Task<ApiResponseEntity<List<UserEntity>>> GetUsersAsync(int pageNumber)
        {
            string cacheKey = $"users_page_{pageNumber}";
            if (_cache.TryGetValue(cacheKey, out ApiResponseEntity<List<UserEntity>> cachedUsers))
            {
                return cachedUsers;
            }
            try
            {
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/users?page={pageNumber}");
                if (!response.IsSuccessStatusCode)
                {
                    throw new UserInfoAppException($"Failed to fetch users for page {pageNumber}. Status: {response.StatusCode}", (int)response.StatusCode);
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponseEntity<List<UserEntity>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.data == null)
                    throw new UserInfoAppException($"Deserialized data is null for page {pageNumber}", 500);


                _cache.Set(cacheKey, apiResponse, _cacheDuration);
                return apiResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new UserInfoAppException("Network error occurred while fetching users.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new UserInfoAppException("The request timed out while fetching users.", ex);
            }
            catch (JsonException ex)
            {
                throw new UserInfoAppException("Failed to deserialize users list.", ex);
            }
        }
    }
}
