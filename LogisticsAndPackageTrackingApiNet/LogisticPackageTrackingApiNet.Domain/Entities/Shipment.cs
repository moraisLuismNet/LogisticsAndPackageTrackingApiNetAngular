using System.ComponentModel.DataAnnotations;

namespace LogisticPackageTrackingApiNet.Domain.Entities;

public class Shipment : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    public string Mail { get; set; } = string.Empty;

    [Required]
    public string OriginAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required]
    public string DestinationAddress { get; set; } = string.Empty;

    public double OriginLatitude { get; set; }

    public double OriginLongitude { get; set; }

    public double DestinationLatitude { get; set; }

    public double DestinationLongitude { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    public decimal Weight { get; set; }
    
    public ICollection<TrackingUpdate> TrackingUpdates { get; set; } = new List<TrackingUpdate>();
}

public enum ShipmentStatus
{
    Pending,
    InTransit,
    Delivered,
    Cancelled
}
