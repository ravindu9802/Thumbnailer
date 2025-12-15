using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Thumbnailer.Application.Services;
using Thumbnailer.Domain.Models;
using Thumbnailer.Domain.Services;

namespace Thumbnailer.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register RabbitMQ Service
        //var factory = new ConnectionFactory { HostName = "localhost" };
        //using var connection = await factory.CreateConnectionAsync();
        //using var channel = await connection.CreateChannelAsync();
        //await channel.QueueDeclareAsync(
        //    queue: "thumbnail_jobs",
        //    durable: true,
        //    exclusive: false,
        //    autoDelete: false,
        //    arguments: null);
        //builder.Services.AddSingleton<IChannel>(channel);

        services.AddSingleton<ConcurrentDictionary<string, ThumbnailGenerationStatus>>();
        services.AddHostedService<ThumbnailGenerationService>();
        services.AddSingleton<IImageService, ImageService>();
        return services;
    }
}
