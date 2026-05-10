using LogisticPackageTrackingApiNet.Application.Helpers;
using LogisticPackageTrackingApiNet.Application.Interfaces;
using Moq;

namespace LogisticPackageTrackingApiNet.UnitTests.Helpers;

public class GeocodingHelperTests
{
    [Fact]
    public void GetOriginCoordinates_ShouldReturnDefaultValues()
    {
        var (lat, lng) = GeocodingHelper.GetOriginCoordinates();

        Assert.Equal(40.4235, lat);
        Assert.Equal(-3.5239, lng);
    }

    [Fact]
    public async Task GetDestinationCoordinates_WithValidAddress_ShouldReturnGeocodedCoordinates()
    {
        var geoMock = new Mock<IGeocodingService>();
        geoMock.Setup(g => g.GetCoordinates("Calle Mayor 10, Madrid")).ReturnsAsync((40.4189, -3.7024));

        var (lat, lng) = await GeocodingHelper.GetDestinationCoordinates("Calle Mayor 10, Madrid", geoMock.Object);

        Assert.Equal(40.4189, lat);
        Assert.Equal(-3.7024, lng);
    }

    [Fact]
    public async Task GetDestinationCoordinates_WithEmptyAddress_ShouldReturnZeroCoordinates()
    {
        var geoMock = new Mock<IGeocodingService>();

        var (lat, lng) = await GeocodingHelper.GetDestinationCoordinates("", geoMock.Object);

        Assert.Equal(0, lat);
        Assert.Equal(0, lng);
    }

    [Fact]
    public async Task GetDestinationCoordinates_WithNullAddress_ShouldReturnZeroCoordinates()
    {
        var geoMock = new Mock<IGeocodingService>();

        var (lat, lng) = await GeocodingHelper.GetDestinationCoordinates(null!, geoMock.Object);

        Assert.Equal(0, lat);
        Assert.Equal(0, lng);
    }
}
