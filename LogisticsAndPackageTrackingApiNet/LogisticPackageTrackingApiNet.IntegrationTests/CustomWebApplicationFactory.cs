using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Application.Messaging;
using LogisticPackageTrackingApiNet.Infrastructure.Messaging;
using LogisticPackageTrackingApiNet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace LogisticPackageTrackingApiNet.IntegrationTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var rabbitDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                     d.ImplementationType == typeof(RabbitMQConsumer));
            if (rabbitDescriptor != null)
                services.Remove(rabbitDescriptor);

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            services.RemoveAll<IMessagePublisher>();
            var publisherMock = new Mock<IMessagePublisher>();
            publisherMock.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            services.AddSingleton<IMessagePublisher>(publisherMock.Object);

            services.RemoveAll<IGeocodingService>();
            var geoMock = new Mock<IGeocodingService>();
            geoMock.Setup(g => g.GetCoordinates(It.IsAny<string>())).ReturnsAsync((40.4168, -3.7038));
            services.AddScoped<IGeocodingService>(_ => geoMock.Object);

            services.RemoveAll<IEmailSender>();
            var emailMock = new Mock<IEmailSender>();
            services.AddScoped<IEmailSender>(_ => emailMock.Object);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
