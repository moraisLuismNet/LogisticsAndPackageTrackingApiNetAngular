using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Domain.Common;

namespace LogisticPackageTrackingApiNet.IntegrationTests;

public class AuthIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task Register_ShouldReturnSuccess()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Jane",
            lastName = "Doe",
            mail = "jane@test.com",
            password = "Pass1234",
            address = "456 Oak Ave"
        });

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(JsonOptions);
        Assert.NotNull(content);
        Assert.True(content.Success);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        await RegisterUser("login-test@test.com", "Pass1234");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            mail = "login-test@test.com",
            password = "Pass1234"
        });

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDTO>>(JsonOptions);
        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data);
        Assert.NotEmpty(content.Data.Token);
        Assert.Equal("login-test@test.com", content.Data.Mail);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        await RegisterUser("badpw@test.com", "Pass1234");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            mail = "badpw@test.com",
            password = "WrongPassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            mail = "nobody@test.com",
            password = "Pass1234"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task RegisterUser(string mail, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            mail,
            password,
            address = "123 Test St"
        });
        response.EnsureSuccessStatusCode();
    }
}
