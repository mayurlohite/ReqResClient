using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using UserInfoApp.Client.Application;
using UserInfoApp.Client.Exceptions;
using UserInfoApp.Client.Infrastructure;
using UserInfoApp.Client.Settings;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); //reading configuration from appsetings
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddMemoryCache();

        services.Configure<ApiConfigs>(configuration.GetSection("ApiConfigs"));

        var apiConfig = new ApiConfigs();
        configuration.GetSection("ApiConfigs").Bind(apiConfig);

        // Register HttpClient for IUserApiClient
        services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiConfig.BaseUrl);
            client.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
        }).AddPolicyHandler(GetRetryPolicy()); ;

        // Register your service layer
        services.AddTransient<IExternalUserService, ExternalUserService>();
    })
    .Build();

// Define retry policy
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 500 + network failures delays, 408 Request Timeout
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)  // checking if its 404
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
            onRetry: (response, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to {response.Result?.StatusCode}");
            });

    //This sets up exponential backoff:

    //1st retry waits 2² = 4 seconds
    //2nd retry waits 2³ = 8 seconds
    //3rd retry waits 2⁴ = 16 seconds
}

// Resolve your service
var userService = host.Services.GetRequiredService<IExternalUserService>();

try
{
    // Fetch single user - from API
    var user = await userService.GetUserByIdAsync(1);
    if (user != null)
    {
        Console.WriteLine($"User: {user.FirstName} {user.LastName}, Email: {user.Email}");
    }

    // Fetch single user - from Cache
    var cacheUser = await userService.GetUserByIdAsync(1);
    if (cacheUser != null)
    {
        Console.WriteLine($"User From Cache: {cacheUser.FirstName} {cacheUser.LastName}, Email: {cacheUser.Email}");
    }

    // Fetch all users with all the pages
    var allUsers = await userService.GetAllUsersAsync();
    if (allUsers != null && allUsers.Count() > 0)
    {
        foreach (var u in allUsers)
        {
            Console.WriteLine($"User: {u.FirstName} {u.LastName}, Email: {u.Email}");
        }
    }

    // Fetch all users with all the pages - from cache
    var cacheAllUser = await userService.GetAllUsersAsync();
    if (cacheAllUser != null && cacheAllUser.Count() > 0)
    {
        foreach (var u in cacheAllUser)
        {
            Console.WriteLine($"Cache User: {u.FirstName} {u.LastName}, Email: {u.Email}");
        }
    }

    Console.ReadKey();
}
catch (UserInfoAppException ex)
{
    Console.WriteLine($"Handled Error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
