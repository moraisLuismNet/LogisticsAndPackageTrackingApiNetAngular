using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Mappings;
using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using LogisticPackageTrackingApiNet.Application.Interfaces;

namespace LogisticPackageTrackingApiNet.Application.Features.Tracking.Handlers;

public interface ITrackingHandler
{
    Task<TrackingUpdateDTO?> AddTrackingUpdate(int shipmentId, string? location, string description, string? ipAddress = null);
    Task<IEnumerable<TrackingUpdateDTO>> GetHistory(int shipmentId);
}

public class TrackingHandler : ITrackingHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeocodingService _geocodingService;
    private readonly INotificationService _notificationService;

    public TrackingHandler(IUnitOfWork unitOfWork, IGeocodingService geocodingService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _geocodingService = geocodingService;
        _notificationService = notificationService;
    }

    public async Task<TrackingUpdateDTO?> AddTrackingUpdate(int shipmentId, string? location, string description, string? ipAddress = null)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);
        if (shipment == null) return null;

        double lat = 0, lng = 0;
        string finalLocation = location ?? string.Empty;

        if (string.IsNullOrWhiteSpace(finalLocation))
        {
            var updates = await _unitOfWork.Tracking.GetByShipmentIdAsync(shipmentId);
            int count = updates.Count();
            
            // Simulation logic: Move 10% each time until 90%
            double progress = Math.Min(0.9, (count + 1) * 0.1);
            
            lat = shipment.OriginLatitude + (shipment.DestinationLatitude - shipment.OriginLatitude) * progress;
            lng = shipment.OriginLongitude + (shipment.DestinationLongitude - shipment.OriginLongitude) * progress;
            
            // Use Reverse Geocoding to get the name of the simulated "middle" location
            finalLocation = await _geocodingService.ReverseGeocode(lat, lng);
            if (string.IsNullOrEmpty(finalLocation)) finalLocation = $"En Route ({progress * 100}%)";
        }
        else
        {
            var coords = await _geocodingService.GetCoordinates(finalLocation);
            lat = coords.Lat;
            lng = coords.Lng;
        }

        var update = new TrackingUpdate
        {
            ShipmentId = shipmentId,
            Location = finalLocation,
            Description = description,
            Latitude = lat,
            Longitude = lng
        };

        if (description.Contains("Delivered", StringComparison.OrdinalIgnoreCase))
        {
            shipment.Status = ShipmentStatus.Delivered;
        }
        else
        {
            shipment.Status = ShipmentStatus.InTransit;
        }

        await _unitOfWork.Tracking.AddAsync(update);
        await _unitOfWork.SaveChangesAsync();

        // Notify user
        await _notificationService.SendTrackingUpdateAsync(shipment.Mail, shipment.TrackingNumber, shipment.Status.ToString(), finalLocation);

        return update.ToDTO();
    }

    public async Task<IEnumerable<TrackingUpdateDTO>> GetHistory(int shipmentId)
    {
        var updates = await _unitOfWork.Tracking.GetByShipmentIdAsync(shipmentId);
        return updates.Select(u => u.ToDTO());
    }
}
