using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Helpers;
using LogisticPackageTrackingApiNet.Domain.Entities;

namespace LogisticPackageTrackingApiNet.Application.Mappings;

public static class ShipmentMapping
{
    public static ShipmentDTO ToDTO(this Shipment shipment)
    {
        return new ShipmentDTO
        {
            Id = shipment.Id,
            TrackingNumber = shipment.TrackingNumber,
            Mail = shipment.Mail,
            OriginAddress = shipment.OriginAddress,
            OriginLatitude = shipment.OriginLatitude,
            OriginLongitude = shipment.OriginLongitude,
            ReceiverName = shipment.ReceiverName,
            DestinationAddress = shipment.DestinationAddress,
            DestinationLatitude = shipment.DestinationLatitude,
            DestinationLongitude = shipment.DestinationLongitude,
            Status = shipment.Status.ToString(),
            Weight = shipment.Weight,
            TrackingUpdates = shipment.TrackingUpdates?.Select(t => t.ToDTO()).ToList() ?? new()
        };
    }

    public static TrackingUpdateDTO ToDTO(this TrackingUpdate trackingUpdate)
    {
        return new TrackingUpdateDTO
        {
            Id = trackingUpdate.Id,
            Location = trackingUpdate.Location,
            Description = trackingUpdate.Description,
            Latitude = trackingUpdate.Latitude,
            Longitude = trackingUpdate.Longitude
        };
    }

    public static Shipment ToEntity(this CreateShipmentDTO dto)
    {
        return new Shipment
        {
            Mail = dto.Mail,
            ReceiverName = dto.ReceiverName,
            DestinationAddress = dto.DestinationAddress,
            Weight = dto.Weight,
            Status = ShipmentStatus.Pending,
            OriginAddress = GeocodingHelper.DefaultOriginAddress,
            OriginLatitude = GeocodingHelper.DefaultOriginLatitude,
            OriginLongitude = GeocodingHelper.DefaultOriginLongitude,
            TrackingNumber = "TEMP"
        };
    }
}
