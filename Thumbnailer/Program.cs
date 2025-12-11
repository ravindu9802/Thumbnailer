using RabbitMQ.Client;
using Scalar.AspNetCore;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Thumbnailer.Hubs;
using Thumbnailer.Models;
using Thumbnailer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "thumbnail_jobs",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

builder.Services.AddSingleton<IChannel>(channel);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton(_ =>
{
    var channel = Channel.CreateBounded<ThumbnailGenerationJob>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    });
    return channel;
});

builder.Services.AddSingleton<ConcurrentDictionary<string, ThumbnailGenerationStatus>>();
builder.Services.AddHostedService<ThumbnailGenerationService>();
builder.Services.AddSingleton<ImageService>();

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed(_ => true)
               .AllowCredentials();
    });
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed(_ => true)
               .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ThumbnailGenerationHub>("hub");

app.UseCors();

app.Run();
