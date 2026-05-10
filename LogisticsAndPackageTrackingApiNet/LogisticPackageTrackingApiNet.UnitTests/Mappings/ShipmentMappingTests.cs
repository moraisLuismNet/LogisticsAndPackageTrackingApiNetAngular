using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Mappings;
using LogisticPackageTrackingApiNet.Domain.Entities;

namespace LogisticPackageTrackingApiNet.UnitTests.Mappings;

public class ShipmentMappingTests
{
    [Fact]
    public void ToDTO_Shipment_ShouldMapAllProperties()
    {
        var shipment = new Shipment
        {
            Id = 1,
            TrackingNumber = "TRK123",
            Mail = "user@test.com",
            OriginAddress = "Origin St",
            OriginLatitude = 10.5,
            OriginLongitude = 20.5,
            ReceiverName = "John",
            DestinationAddress = "Dest Ave",
            DestinationLatitude = 30.5,
            DestinationLongitude = 40.5,
            Status = ShipmentStatus.InTransit,
            Weight = 3.5m
        };

        var dto = shipment.ToDTO();

        Assert.Equal(1, dto.Id);
        Assert.Equal("TRK123", dto.TrackingNumber);
        Assert.Equal("user@test.com", dto.Mail);
        Assert.Equal("Origin St", dto.OriginAddress);
        Assert.Equal(10.5, dto.OriginLatitude);
        Assert.Equal(20.5, dto.OriginLongitude);
        Assert.Equal("John", dto.ReceiverName);
        Assert.Equal("Dest Ave", dto.DestinationAddress);
        Assert.Equal(30.5, dto.DestinationLatitude);
        Assert.Equal(40.5, dto.DestinationLongitude);
        Assert.Equal("InTransit", dto.Status);
        Assert.Equal(3.5m, dto.Weight);
    }

    [Fact]
    public void ToDTO_Shipment_WithTrackingUpdates_ShouldMapUpdates()
    {
        var shipment = new Shipment
        {
            Id = 1,
            TrackingNumber = "TRK123",
            Mail = "user@test.com",
            TrackingUpdates = new List<TrackingUpdate>
            {
                new() { Id = 10, Location = "A", Description = "Scan", Latitude = 1.0, Longitude = 2.0 }
            }
        };

        var dto = shipment.ToDTO();

        Assert.Single(dto.TrackingUpdates);
        Assert.Equal(10, dto.TrackingUpdates[0].Id);
        Assert.Equal("A", dto.TrackingUpdates[0].Location);
        Assert.Equal("Scan", dto.TrackingUpdates[0].Description);
        Assert.Equal(1.0, dto.TrackingUpdates[0].Latitude);
        Assert.Equal(2.0, dto.TrackingUpdates[0].Longitude);
    }

    [Fact]
    public void ToDTO_Shipment_WithoutTrackingUpdates_ShouldReturnEmptyList()
    {
        var shipment = new Shipment
        {
            Id = 2,
            TrackingNumber = "TRK456",
            Mail = "user@test.com"
        };

        var dto = shipment.ToDTO();

        Assert.NotNull(dto.TrackingUpdates);
        Assert.Empty(dto.TrackingUpdates);
    }

    [Fact]
    public void ToDTO_TrackingUpdate_ShouldMapAllProperties()
    {
        var update = new TrackingUpdate
        {
            Id = 5,
            Location = "Madrid Hub",
            Description = "Arrived at hub",
            Latitude = 40.4168,
            Longitude = -3.7038
        };

        var dto = update.ToDTO();

        Assert.Equal(5, dto.Id);
        Assert.Equal("Madrid Hub", dto.Location);
        Assert.Equal("Arrived at hub", dto.Description);
        Assert.Equal(40.4168, dto.Latitude);
        Assert.Equal(-3.7038, dto.Longitude);
    }

    [Fact]
    public void ToEntity_CreateShipmentDTO_ShouldMapCorrectly()
    {
        var dto = new CreateShipmentDTO
        {
            Mail = "newuser@test.com",
            ReceiverName = "Alice",
            DestinationAddress = "123 Main St",
            Weight = 2.0m
        };

        var entity = dto.ToEntity();

        Assert.Equal("newuser@test.com", entity.Mail);
        Assert.Equal("Alice", entity.ReceiverName);
        Assert.Equal("123 Main St", entity.DestinationAddress);
        Assert.Equal(2.0m, entity.Weight);
        Assert.Equal(ShipmentStatus.Pending, entity.Status);
        Assert.Equal("Avda. José Garate, 11, Alcalá de Henares", entity.OriginAddress);
        Assert.Equal(40.4235, entity.OriginLatitude);
        Assert.Equal(-3.5239, entity.OriginLongitude);
        Assert.Equal("TEMP", entity.TrackingNumber);
    }
}
