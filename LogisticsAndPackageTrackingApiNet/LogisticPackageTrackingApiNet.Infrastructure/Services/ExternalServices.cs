using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net.Http;
using System.Globalization;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Application.Messaging;
using LogisticPackageTrackingApiNet.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace LogisticPackageTrackingApiNet.Infrastructure.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "logistic_tracking"
        };
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }
}

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(HttpClient httpClient, IConfiguration configuration, ILogger<BrevoEmailSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var apiKey = _configuration["EmailConfiguration:BrevoApiKey"];
        var fromEmail = _configuration["EmailConfiguration:FromEmail"];
        var fromName = _configuration["EmailConfiguration:FromName"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Brevo API key not configured, email not sent");
            return;
        }

        var payload = new
        {
            sender = new { email = fromEmail, name = fromName },
            to = new[] { new { email = to } },
            subject,
            htmlContent = body
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", apiKey);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send email to {To}. Status: {Status}, Error: {Error}", to, response.StatusCode, error);
        }
    }
}

public class NotificationService : INotificationService
{
    private readonly IMessagePublisher _publisher;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IMessagePublisher publisher, IEmailSender emailSender, ILogger<NotificationService> logger)
    {
        _publisher = publisher;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new EmailMessage { To = to, Subject = subject, Body = body };

        try
        {
            await _publisher.PublishAsync(message, "email_queue");
            _logger.LogInformation("Email queued for {To}: {Subject}", to, subject);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ unavailable, sending email directly");
        }

        await _emailSender.SendEmailAsync(to, subject, body);
    }

    public async Task SendTrackingUpdateAsync(string email, string trackingNumber, string status, string location)
    {
        string subject = $"Update for Shipment {trackingNumber}";
        string body = $"Your shipment is now {status} at {location}.";
        await SendEmailAsync(email, subject, body);
    }
}

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _countryCodes;

    public GeocodingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenStreetMap:ApiKey"] ?? string.Empty;
        _countryCodes = configuration["OpenStreetMap:CountryCodes"] ?? string.Empty;
        
        // Nominatim requires a User-Agent header (set once)
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LogisticPackageTrackingApiNet");
        }
    }

    public async Task<(double Lat, double Lng)> GetCoordinates(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return (0, 0);

        // Nominatim (OpenStreetMap) Geocoding API or OSM-based providers
        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
        
        if (!string.IsNullOrEmpty(_apiKey))
        {
            url += $"&key={_apiKey}";
        }

        if (!string.IsNullOrEmpty(_countryCodes))
        {
            url += $"&countrycodes={_countryCodes}";
        }
        
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return (0, 0);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
        {
            var firstResult = root[0];
            string latStr = firstResult.GetProperty("lat").GetString() ?? "0";
            string lonStr = firstResult.GetProperty("lon").GetString() ?? "0";

            if (double.TryParse(latStr, CultureInfo.InvariantCulture, out double lat) &&
                double.TryParse(lonStr, CultureInfo.InvariantCulture, out double lon))
            {
                return (lat, lon);
            }
        }

        return (0, 0);
    }

    public async Task<(double Lat, double Lng, string City)> GetLocationFromIp(string ipAddress)
    {
        // IP-API (free for non-commercial use)
        // If it's localhost or private, it might fail, so we return a default (Gernika as requested)
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1") 
            return (43.3122, -2.6804, "Gernika-Lumo (Local)");

        try
        {
            var url = $"http://ip-api.com/json/{ipAddress}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return (0, 0, "Unknown");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.GetProperty("status").GetString() == "success")
            {
                var lat = root.GetProperty("lat").GetDouble();
                var lon = root.GetProperty("lon").GetDouble();
                var city = root.GetProperty("city").GetString() ?? "Unknown";

                // Enrich with accurate address from OSM Reverse Geocoding
                var accurateAddress = await ReverseGeocode(lat, lon);
                
                return (lat, lon, string.IsNullOrEmpty(accurateAddress) ? city : accurateAddress);
            }
        }
        catch
        {
            // Fallback
        }

        return (0, 0, "Unknown");
    }

    public async Task<string> ReverseGeocode(double lat, double lng)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lng.ToString(CultureInfo.InvariantCulture)}&zoom=18&addressdetails=1";
        
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return string.Empty;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            return root.GetProperty("display_name").GetString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public Task<double> CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        // Haversine formula for calculating the actual distance between two points (in km)
        var R = 6371; // Average radius of the Earth in km
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
                
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Task.FromResult(R * c);
    }

    private static double ToRadians(double angle) => Math.PI * angle / 180.0;
}
