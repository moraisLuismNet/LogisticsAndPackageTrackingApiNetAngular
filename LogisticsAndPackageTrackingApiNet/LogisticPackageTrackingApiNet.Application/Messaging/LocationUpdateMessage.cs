namespace LogisticPackageTrackingApiNet.Application.Messaging;

public record LocationUpdateMessage
{
    public string TrackingNumber { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Location { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
