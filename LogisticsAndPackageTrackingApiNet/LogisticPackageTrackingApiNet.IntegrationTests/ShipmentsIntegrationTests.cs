using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Domain.Common;

namespace LogisticPackageTrackingApiNet.IntegrationTests;

public class ShipmentsIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ShipmentsIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task GetByTrackingNumber_WithNonExistentNumber_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/shipments/DOESNOTEXIST");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateShipment_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", new
        {
            mail = "test@test.com",
            receiverName = "Bob",
            destinationAddress = "789 Pine St",
            weight = 3.0
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateShipment_And_GetByTrackingNumber_ShouldWork()
    {
        var token = await GetTokenAsync("ship-flow@test.com", "Pass1234");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/shipments", new
        {
            mail = "ship-flow@test.com",
            receiverName = "Alice Johnson",
            destinationAddress = "123 Main St, Madrid",
            weight = 2.5m
        });
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponse<ShipmentDTO>>(JsonOptions);
        Assert.NotNull(createContent);
        Assert.True(createContent.Success);
        Assert.NotNull(createContent.Data);
        var trackingNumber = createContent.Data.TrackingNumber;
        Assert.NotEmpty(trackingNumber);
        Assert.Equal(2.5m, createContent.Data.Weight);
        Assert.Equal("ship-flow@test.com", createContent.Data.Mail);

        _client.DefaultRequestHeaders.Authorization = null;

        var getResponse = await _client.GetAsync($"/api/shipments/{trackingNumber}");
        getResponse.EnsureSuccessStatusCode();

        var getContent = await getResponse.Content
            .ReadFromJsonAsync<ApiResponse<ShipmentDTO>>(JsonOptions);
        Assert.NotNull(getContent);
        Assert.True(getContent.Success);
        Assert.NotNull(getContent.Data);
        Assert.Equal(trackingNumber, getContent.Data.TrackingNumber);
        Assert.Equal("123 Main St, Madrid", getContent.Data.DestinationAddress);
    }

    [Fact]
    public async Task UpdateStatus_AsAdmin_ShouldUpdateAndReturnNewStatus()
    {
        var token = await GetTokenAsync("status-flow@test.com", "Pass1234");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/shipments", new
        {
            mail = "status-flow@test.com",
            receiverName = "Carlos Ruiz",
            destinationAddress = "Gran Vía 30, Madrid",
            weight = 1.2m
        });
        createResponse.EnsureSuccessStatusCode();
        var createContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponse<ShipmentDTO>>(JsonOptions);
        var trackingNumber = createContent!.Data!.TrackingNumber;

        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var updateResponse = await _client.PutAsJsonAsync($"/api/shipments/{trackingNumber}/status", new
        {
            status = "InTransit"
        });
        updateResponse.EnsureSuccessStatusCode();

        var updateContent = await updateResponse.Content
            .ReadFromJsonAsync<ApiResponse<ShipmentDTO>>(JsonOptions);
        Assert.NotNull(updateContent);
        Assert.True(updateContent.Success);
        Assert.NotNull(updateContent.Data);
        Assert.Equal("InTransit", updateContent.Data.Status);
    }

    private async Task<string> GetTokenAsync(string mail, string password)
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            mail,
            password,
            address = "123 Test St"
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            mail,
            password
        });
        loginResponse.EnsureSuccessStatusCode();

        var content = await loginResponse.Content
            .ReadFromJsonAsync<ApiResponse<AuthResponseDTO>>(JsonOptions);
        return content!.Data!.Token;
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            mail = "luis@mail.com",
            password = "123456"
        });
        loginResponse.EnsureSuccessStatusCode();

        var content = await loginResponse.Content
            .ReadFromJsonAsync<ApiResponse<AuthResponseDTO>>(JsonOptions);
        return content!.Data!.Token;
    }
}
