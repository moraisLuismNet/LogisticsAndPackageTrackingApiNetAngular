using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Helpers;
using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Application.Mappings;
using LogisticPackageTrackingApiNet.Application.Messaging;
using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;

namespace LogisticPackageTrackingApiNet.Application.Features.Shipments.Handlers;

public interface IShipmentHandler
{
    Task<ShipmentDTO?> CreateShipment(CreateShipmentDTO createDto);
    Task<ShipmentDTO?> GetShipmentById(int id);
    Task<ShipmentDTO?> GetShipmentByTrackingNumber(string trackingNumber);
    Task<IEnumerable<ShipmentDTO>> GetAllShipments();
    Task<IEnumerable<ShipmentDTO>> GetAllShipmentsByMailAsync(string mail);
    Task<ShipmentDTO?> UpdateStatus(string trackingNumber, UpdateShipmentStatusDTO dto);
}

public class ShipmentHandler : IShipmentHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeocodingService _geocodingService;
    private readonly INotificationService _notificationService;
    private readonly IMessagePublisher _publisher;

    public ShipmentHandler(IUnitOfWork unitOfWork, IGeocodingService geocodingService, INotificationService notificationService, IMessagePublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _geocodingService = geocodingService;
        _notificationService = notificationService;
        _publisher = publisher;
    }

    public async Task<ShipmentDTO?> CreateShipment(CreateShipmentDTO createDto)
    {
        var shipment = createDto.ToEntity();

        var todayCount = await GetTodayShipmentCount();
        shipment.TrackingNumber = GenerateTrackingNumber(todayCount + 1);

        var (originLat, originLng) = GeocodingHelper.GetOriginCoordinates();
        shipment.OriginLatitude = originLat;
        shipment.OriginLongitude = originLng;

        var (destLat, destLng) = await GeocodingHelper.GetDestinationCoordinates(createDto.DestinationAddress, _geocodingService);
        shipment.DestinationLatitude = destLat;
        shipment.DestinationLongitude = destLng;

        await _unitOfWork.Shipments.AddAsync(shipment);
        await _unitOfWork.SaveChangesAsync();
        return shipment.ToDTO();
    }

    public async Task<ShipmentDTO?> GetShipmentById(int id)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(id);
        return shipment?.ToDTO();
    }

    public async Task<ShipmentDTO?> GetShipmentByTrackingNumber(string trackingNumber)
    {
        var shipment = await _unitOfWork.Shipments.GetByTrackingNumberAsync(trackingNumber);
        return shipment?.ToDTO();
    }

    public async Task<IEnumerable<ShipmentDTO>> GetAllShipments()
    {
        var shipments = await _unitOfWork.Shipments.GetAllAsync();
        return shipments.Select(s => s.ToDTO());
    }

    public async Task<IEnumerable<ShipmentDTO>> GetAllShipmentsByMailAsync(string mail)
    {
        var shipments = await _unitOfWork.Shipments.GetByMailAsync(mail);
        return shipments.Select(s => s.ToDTO());
    }

    public async Task<ShipmentDTO?> UpdateStatus(string trackingNumber, UpdateShipmentStatusDTO dto)
    {
        var shipment = await _unitOfWork.Shipments.GetByTrackingNumberAsync(trackingNumber);
        if (shipment == null) return null;

        shipment.Status = dto.Status;
        _unitOfWork.Shipments.Update(shipment);

        double lat, lng;
        string location;

        if (dto.Status == ShipmentStatus.Delivered)
        {
            lat = shipment.DestinationLatitude;
            lng = shipment.DestinationLongitude;
            location = shipment.DestinationAddress;
        }
        else if (dto.Status == ShipmentStatus.InTransit)
        {
            lat = (shipment.OriginLatitude + shipment.DestinationLatitude) / 2;
            lng = (shipment.OriginLongitude + shipment.DestinationLongitude) / 2;
            location = "In Transit";
        }
        else
        {
            lat = shipment.OriginLatitude;
            lng = shipment.OriginLongitude;
            location = shipment.OriginAddress;
        }

        var update = new TrackingUpdate
        {
            ShipmentId = shipment.Id,
            Location = location,
            Description = $"Status changed to {dto.Status}",
            Latitude = lat,
            Longitude = lng
        };
        await _unitOfWork.Tracking.AddAsync(update);
        await _unitOfWork.SaveChangesAsync();

        var locationMsg = new LocationUpdateMessage
        {
            TrackingNumber = shipment.TrackingNumber,
            Latitude = lat,
            Longitude = lng,
            Location = location,
            Status = dto.Status.ToString()
        };
        try
        {
            await _publisher.PublishAsync(locationMsg, "location_queue");
        }
        catch
        {
            // Location publish is non-critical; email notification still sent via fallback
        }

        if (dto.Status == ShipmentStatus.Delivered)
        {
            await _notificationService.SendTrackingUpdateAsync(shipment.Mail, shipment.TrackingNumber, "Delivered", shipment.DestinationAddress);
        }

        return shipment.ToDTO();
    }

    private static string GenerateTrackingNumber(int daySequence)
    {
        var now = DateTime.UtcNow;
        var day = now.ToString("dd");
        var month = now.ToString("MMM").ToUpper()[..2];
        var year = now.ToString("yy");
        var randomLetters = Guid.NewGuid().ToString().ToUpper().Replace("-", "").Substring(0, 3);
        return $"{day}{month}{year}{randomLetters}-{daySequence}";
    }

    private async Task<int> GetTodayShipmentCount()
    {
        var all = await _unitOfWork.Shipments.GetAllAsync();
        return all.Count();
    }
}
