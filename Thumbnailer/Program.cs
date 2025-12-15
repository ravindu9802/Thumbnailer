using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using System.Reflection;
using System.Threading.Channels;
using Thumbnailer.Application;
using Thumbnailer.Application.Hubs;
using Thumbnailer.Domain.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Register MediatR Service
builder.Services.AddMediatR(
    cfg => cfg.RegisterServicesFromAssemblies(
        [Assembly.GetAssembly(typeof(ApplicationServiceExtensions))!]));

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

builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Forward Api Gateway Protocol and Host to mask original Scheme and Host
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ThumbnailGenerationHub>("hub");

app.UseCors();

app.Run();
