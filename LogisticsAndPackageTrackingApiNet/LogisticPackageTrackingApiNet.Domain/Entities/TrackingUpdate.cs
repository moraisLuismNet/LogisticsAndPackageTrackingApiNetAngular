using System.ComponentModel.DataAnnotations;

namespace LogisticPackageTrackingApiNet.Domain.Entities;

public class TrackingUpdate : BaseEntity
{
    public int ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    [Required]
    public string Location { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
