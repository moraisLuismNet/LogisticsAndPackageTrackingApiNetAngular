using LogisticPackageTrackingApiNet.Domain.Entities;

namespace LogisticPackageTrackingApiNet.Application.DTOs;

public record ShipmentDTO
{
    public int Id { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public string Mail { get; init; } = string.Empty;
    public string OriginAddress { get; init; } = string.Empty;
    public double OriginLatitude { get; init; }
    public double OriginLongitude { get; init; }
    public string ReceiverName { get; init; } = string.Empty;
    public string DestinationAddress { get; init; } = string.Empty;
    public double DestinationLatitude { get; init; }
    public double DestinationLongitude { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public List<TrackingUpdateDTO> TrackingUpdates { get; init; } = new();
}

public record TrackingUpdateDTO
{
    public int Id { get; init; }
    public string Location { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public record AddTrackingUpdateDTO
{
    public string Description { get; init; } = string.Empty;
}

public record CreateShipmentDTO
{
    public string Mail { get; init; } = string.Empty;
    public string ReceiverName { get; init; } = string.Empty;
    public string DestinationAddress { get; init; } = string.Empty;
    public decimal Weight { get; init; }
}

public record UpdateShipmentStatusDTO
{
    public ShipmentStatus Status { get; init; }
}
