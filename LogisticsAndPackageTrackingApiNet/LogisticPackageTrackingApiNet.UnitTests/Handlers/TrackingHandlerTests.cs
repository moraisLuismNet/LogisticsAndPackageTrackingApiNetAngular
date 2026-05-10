using LogisticPackageTrackingApiNet.Application.Features.Tracking.Handlers;
using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using Moq;

namespace LogisticPackageTrackingApiNet.UnitTests.Handlers;

public class TrackingHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IGeocodingService> _geoMock;
    private readonly Mock<INotificationService> _notifMock;
    private readonly TrackingHandler _handler;

    public TrackingHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _geoMock = new Mock<IGeocodingService>();
        _notifMock = new Mock<INotificationService>();

        _geoMock.Setup(g => g.GetCoordinates(It.IsAny<string>())).ReturnsAsync((40.4168, -3.7038));
        _geoMock.Setup(g => g.ReverseGeocode(It.IsAny<double>(), It.IsAny<double>())).ReturnsAsync("Madrid, Spain");

        _handler = new TrackingHandler(_uowMock.Object, _geoMock.Object, _notifMock.Object);
    }

    [Fact]
    public async Task AddTrackingUpdate_WithNonExistingShipment_ShouldReturnNull()
    {
        _uowMock.Setup(u => u.Shipments.GetByIdAsync(99)).ReturnsAsync((Shipment?)null);

        var result = await _handler.AddTrackingUpdate(99, "Warehouse", "Scanned");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddTrackingUpdate_WithExistingShipment_ShouldAddUpdateAndReturnDTO()
    {
        var shipment = new Shipment
        {
            Id = 1,
            TrackingNumber = "TRK001",
            OriginLatitude = 10.0,
            OriginLongitude = 10.0,
            DestinationLatitude = 20.0,
            DestinationLongitude = 20.0,
            Mail = "user@test.com"
        };
        _uowMock.Setup(u => u.Shipments.GetByIdAsync(1)).ReturnsAsync(shipment);
        _uowMock.Setup(u => u.Tracking.GetByShipmentIdAsync(1)).ReturnsAsync(new List<TrackingUpdate>());
        _uowMock.Setup(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _notifMock.Setup(n => n.SendTrackingUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _handler.AddTrackingUpdate(1, "Madrid Warehouse", "Package arrived at hub");

        Assert.NotNull(result);
        Assert.Equal("Madrid Warehouse", result.Location);
        Assert.Equal("Package arrived at hub", result.Description);
        _uowMock.Verify(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _notifMock.Verify(n => n.SendTrackingUpdateAsync("user@test.com", "TRK001", It.IsAny<string>(), "Madrid Warehouse"), Times.Once);
    }

    [Fact]
    public async Task AddTrackingUpdate_WhenDescriptionContainsDelivered_ShouldSetShipmentStatusToDelivered()
    {
        var shipment = new Shipment
        {
            Id = 2,
            TrackingNumber = "TRK002",
            OriginLatitude = 10.0,
            OriginLongitude = 10.0,
            DestinationLatitude = 20.0,
            DestinationLongitude = 20.0,
            Mail = "user@test.com",
            Status = ShipmentStatus.InTransit
        };
        _uowMock.Setup(u => u.Shipments.GetByIdAsync(2)).ReturnsAsync(shipment);
        _uowMock.Setup(u => u.Tracking.GetByShipmentIdAsync(2)).ReturnsAsync(new List<TrackingUpdate>());
        _uowMock.Setup(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _notifMock.Setup(n => n.SendTrackingUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _handler.AddTrackingUpdate(2, null, "Package Delivered successfully");

        Assert.NotNull(result);
        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        _notifMock.Verify(n => n.SendTrackingUpdateAsync("user@test.com", "TRK002", "Delivered", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddTrackingUpdate_WithoutLocation_ShouldSimulateProgress()
    {
        var shipment = new Shipment
        {
            Id = 3,
            TrackingNumber = "TRK003",
            OriginLatitude = 0.0,
            OriginLongitude = 0.0,
            DestinationLatitude = 10.0,
            DestinationLongitude = 10.0,
            Mail = "user@test.com"
        };
        _uowMock.Setup(u => u.Shipments.GetByIdAsync(3)).ReturnsAsync(shipment);
        _uowMock.Setup(u => u.Tracking.GetByShipmentIdAsync(3)).ReturnsAsync(new List<TrackingUpdate>());
        _uowMock.Setup(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _notifMock.Setup(n => n.SendTrackingUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _handler.AddTrackingUpdate(3, null, "In transit");

        Assert.NotNull(result);
        Assert.Equal(1.0, result.Latitude);
        Assert.Equal(1.0, result.Longitude);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnAllUpdatesForShipment()
    {
        var updates = new List<TrackingUpdate>
        {
            new() { Id = 1, ShipmentId = 1, Location = "A", Description = "First" },
            new() { Id = 2, ShipmentId = 1, Location = "B", Description = "Second" }
        };
        _uowMock.Setup(u => u.Tracking.GetByShipmentIdAsync(1)).ReturnsAsync(updates);

        var result = await _handler.GetHistory(1);

        Assert.Equal(2, result.Count());
    }
}
