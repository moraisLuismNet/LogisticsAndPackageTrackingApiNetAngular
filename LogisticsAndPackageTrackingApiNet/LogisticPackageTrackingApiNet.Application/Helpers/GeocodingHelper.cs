using LogisticPackageTrackingApiNet.Application.Interfaces;

namespace LogisticPackageTrackingApiNet.Application.Helpers;

public static class GeocodingHelper
{
    public const string DefaultOriginAddress = "Avda. José Garate, 11, Alcalá de Henares";
    public const double DefaultOriginLatitude = 40.4235;
    public const double DefaultOriginLongitude = -3.5239;

    public static (double Lat, double Lng) GetOriginCoordinates()
    {
        return (DefaultOriginLatitude, DefaultOriginLongitude);
    }

    public static async Task<(double Lat, double Lng)> GetDestinationCoordinates(string address, IGeocodingService geocodingService)
    {
        if (string.IsNullOrWhiteSpace(address))
            return (0, 0);

        return await geocodingService.GetCoordinates(address);
    }
}
