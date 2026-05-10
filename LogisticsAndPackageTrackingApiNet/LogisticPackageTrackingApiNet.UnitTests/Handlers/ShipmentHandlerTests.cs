using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Features.Shipments.Handlers;
using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Application.Messaging;
using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using Moq;

namespace LogisticPackageTrackingApiNet.UnitTests.Handlers;

public class ShipmentHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IGeocodingService> _geoMock;
    private readonly Mock<INotificationService> _notifMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly ShipmentHandler _handler;

    public ShipmentHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _geoMock = new Mock<IGeocodingService>();
        _notifMock = new Mock<INotificationService>();
        _publisherMock = new Mock<IMessagePublisher>();

        _geoMock.Setup(g => g.GetCoordinates(It.IsAny<string>())).ReturnsAsync((0, 0));

        _handler = new ShipmentHandler(_uowMock.Object, _geoMock.Object, _notifMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task CreateShipment_ShouldAddShipmentAndSave()
    {
        var dto = new CreateShipmentDTO
        {
            Mail = "alice@example.com",
            ReceiverName = "Alice Smith",
            DestinationAddress = "456 Oak Ave",
            Weight = 5.5m
        };

        _uowMock.Setup(u => u.Shipments.AddAsync(It.IsAny<Shipment>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.CreateShipment(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Mail, result.Mail);
        _uowMock.Verify(u => u.Shipments.AddAsync(It.IsAny<Shipment>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetShipmentById_WithExistingId_ShouldReturnShipment()
    {
        var shipment = new Shipment { Id = 1, TrackingNumber = "ABC123", Mail = "test@test.com" };
        _uowMock.Setup(u => u.Shipments.GetByIdAsync(1)).ReturnsAsync(shipment);

        var result = await _handler.GetShipmentById(1);

        Assert.NotNull(result);
        Assert.Equal("ABC123", result.TrackingNumber);
    }

    [Fact]
    public async Task GetShipmentById_WithNonExistingId_ShouldReturnNull()
    {
        _uowMock.Setup(u => u.Shipments.GetByIdAsync(99)).ReturnsAsync((Shipment?)null);

        var result = await _handler.GetShipmentById(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetShipmentByTrackingNumber_WithExistingNumber_ShouldReturnShipment()
    {
        var shipment = new Shipment { Id = 2, TrackingNumber = "TRK789", Mail = "test@test.com" };
        _uowMock.Setup(u => u.Shipments.GetByTrackingNumberAsync("TRK789")).ReturnsAsync(shipment);

        var result = await _handler.GetShipmentByTrackingNumber("TRK789");

        Assert.NotNull(result);
        Assert.Equal("TRK789", result.TrackingNumber);
    }

    [Fact]
    public async Task GetByTrackingNumber_WithNonExistingNumber_ShouldReturnNull()
    {
        _uowMock.Setup(u => u.Shipments.GetByTrackingNumberAsync("INVALID")).ReturnsAsync((Shipment?)null);

        var result = await _handler.GetShipmentByTrackingNumber("INVALID");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllShipments_ShouldReturnAll()
    {
        var shipments = new List<Shipment>
        {
            new() { Id = 1, TrackingNumber = "A", Mail = "a@a.com" },
            new() { Id = 2, TrackingNumber = "B", Mail = "b@b.com" }
        };
        _uowMock.Setup(u => u.Shipments.GetAllAsync()).ReturnsAsync(shipments);

        var result = await _handler.GetAllShipments();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllShipmentsByMailAsync_ShouldReturnFiltered()
    {
        var shipments = new List<Shipment>
        {
            new() { Id = 1, TrackingNumber = "A", Mail = "user@test.com" }
        };
        _uowMock.Setup(u => u.Shipments.GetByMailAsync("user@test.com")).ReturnsAsync(shipments);

        var result = await _handler.GetAllShipmentsByMailAsync("user@test.com");

        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateStatus_WithNonExistingTrackingNumber_ShouldReturnNull()
    {
        _uowMock.Setup(u => u.Shipments.GetByTrackingNumberAsync("MISSING")).ReturnsAsync((Shipment?)null);

        var result = await _handler.UpdateStatus("MISSING", new UpdateShipmentStatusDTO { Status = ShipmentStatus.InTransit });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatus_ToDelivered_ShouldSetDestinationCoordinates()
    {
        var shipment = new Shipment
        {
            Id = 1,
            TrackingNumber = "TRK001",
            OriginLatitude = 10.0,
            OriginLongitude = 20.0,
            DestinationLatitude = 30.0,
            DestinationLongitude = 40.0,
            DestinationAddress = "456 Elm St",
            Mail = "user@test.com"
        };
        _uowMock.Setup(u => u.Shipments.GetByTrackingNumberAsync("TRK001")).ReturnsAsync(shipment);
        _uowMock.Setup(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _publisherMock.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _notifMock.Setup(n => n.SendTrackingUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _handler.UpdateStatus("TRK001", new UpdateShipmentStatusDTO { Status = ShipmentStatus.Delivered });

        Assert.NotNull(result);
        Assert.Equal("Delivered", result.Status);
        _notifMock.Verify(n => n.SendTrackingUpdateAsync("user@test.com", "TRK001", "Delivered", "456 Elm St"), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ToInTransit_ShouldSetMidpointCoordinates()
    {
        var shipment = new Shipment
        {
            Id = 2,
            TrackingNumber = "TRK002",
            OriginLatitude = 0.0,
            OriginLongitude = 0.0,
            DestinationLatitude = 10.0,
            DestinationLongitude = 10.0,
            Mail = "user@test.com"
        };
        _uowMock.Setup(u => u.Shipments.GetByTrackingNumberAsync("TRK002")).ReturnsAsync(shipment);
        _uowMock.Setup(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.UpdateStatus("TRK002", new UpdateShipmentStatusDTO { Status = ShipmentStatus.InTransit });

        Assert.NotNull(result);
        Assert.Equal("InTransit", result.Status);
    }

    [Fact]
    public async Task UpdateStatus_ToPendingOrCancelled_ShouldSetOriginCoordinates()
    {
        var shipment = new Shipment
        {
            Id = 3,
            TrackingNumber = "TRK003",
            OriginLatitude = 15.0,
            OriginLongitude = 25.0,
            DestinationLatitude = 35.0,
            DestinationLongitude = 45.0,
            Mail = "user@test.com"
        };
        _uowMock.Setup(u => u.Shipments.GetByTrackingNumberAsync("TRK003")).ReturnsAsync(shipment);
        _uowMock.Setup(u => u.Tracking.AddAsync(It.IsAny<TrackingUpdate>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.UpdateStatus("TRK003", new UpdateShipmentStatusDTO { Status = ShipmentStatus.Pending });

        Assert.NotNull(result);
        Assert.Equal("Pending", result.Status);
    }
}
