# UserInfoApp (.NET 8) 👨‍💻

A clean, extensible, and resilient .NET 8 solution for integrating with external APIs. This project uses the [ReqRes](https://reqres.in) public API to fetch and manage user information while following Clean Architecture principles, proper layering, async patterns, caching, and error handling.

---

## 📦 Solution Structure

This solution consists of **3 projects**:

### 1. **UserInfoApp.Client** (Class Library)
> This is the core of the solution, containing the business logic, domain models, and integration logic.

This library follows **Clean Architecture** and includes the following layers:

- **Domains**
  - Contains entity classes directly mapped from the external API.
  - These mirror the original JSON structure from endpoints like `https://reqres.in/api/users/{userId}`.
  
- **Models (DTOs)**
  - These represent internal project-specific Data Transfer Objects.
  - They follow .NET naming conventions and act as a contract between layers.
  
- **Infrastructure**
  - Handles low-level HTTP interactions with the ReqRes API using `HttpClient`.
  - Implements in-memory **caching** of responses for performance (via `IMemoryCache`).
  - Responsible for throwing structured exceptions when API calls fail.

- **Application**
  - Contains higher-level business logic.
  - Determines *when* and *how* to call infrastructure services.
  - Exposes user-friendly service methods like:
    - `Task<UserDto> GetUserByIdAsync(int userId)`
    - `Task<IEnumerable<UserDto>> GetAllUsersAsync()`

- **Exception**
  - Contains custom exception classes to distinguish between failure scenarios (e.g., `ApiException`, `UserNotFoundException`).

---

### 2. **UserInfoApp.ConsoleApp** (Console UI)

> Demonstrates how to consume services from `UserInfoApp.Client`.

- Uses **HttpClientFactory** for dependency-injected, typed HTTP clients.
- Integrates **Polly** (via `Microsoft.Extensions.Http.Polly`) for **resilient retry policies**:
  - Retries on transient failures (e.g., network issues).
  - Retry pattern: 3 attempts — after 4, 8, and 16 seconds.
- Fetches and displays:
  - Details of a single user (both from API and cached result).
  - List of all users (paginated internally).
- Showcases both success and fallback behavior using clean logging and console output.

---

### 3. **UserInfoApp.Tests** (xUnit Test Project)

> Unit tests to verify the correctness and robustness of the service logic.

Test Scenarios:
- **ReturnsUser_WhenApiCallSucceeds**  
  Ensures a user is returned correctly when the API responds successfully.
  
- **ThrowsApiException_WhenApiCallFails**  
  Validates custom exception handling when API fails or returns a non-success response.
  
- **ReturnsAllUsers_WhenPaginatedApiCallSucceeds**  
  Ensures pagination is handled correctly and all users are fetched from all available pages.

---

## 🛠 Tech Stack

- [.NET 8](https://dotnet.microsoft.com/en-us/)
- **HttpClientFactory**
- **Microsoft.Extensions.Http.Polly** (Retry policies)
- **IMemoryCache** (In-memory caching)
- **xUnit** (Unit testing)
- **Clean Architecture**

---

## 🚀 How to Run

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/UserInfoApp.git
   cd UserInfoApp
