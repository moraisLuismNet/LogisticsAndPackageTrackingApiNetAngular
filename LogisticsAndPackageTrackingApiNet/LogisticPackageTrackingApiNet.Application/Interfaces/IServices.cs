namespace LogisticPackageTrackingApiNet.Application.Interfaces;

public interface IGeocodingService
{
    Task<(double Lat, double Lng)> GetCoordinates(string address);
    Task<(double Lat, double Lng, string City)> GetLocationFromIp(string ipAddress);
    Task<string> ReverseGeocode(double lat, double lng);
    Task<double> CalculateDistance(double lat1, double lng1, double lat2, double lng2);
}

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendTrackingUpdateAsync(string email, string trackingNumber, string status, string location);
}

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}
