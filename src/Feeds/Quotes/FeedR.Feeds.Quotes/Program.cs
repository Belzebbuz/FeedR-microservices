using FeedR.Feeds.Quotes.Pricing.Requests;
using FeedR.Feeds.Quotes.Pricing.Services;
using FeedR.Shared.Redis;
using FeedR.Shared.Redis.Streaming;
using FeedR.Shared.Serialization;
using FeedR.Shared.Streaming;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddSerialization()
	.AddStreaming()
	.AddRedis(builder.Configuration)
	.AddRedisStreaming()
	.AddSingleton<PricingRequestChannel>()
	.AddSingleton<IPricingGenerator, PricingGenerator>()
	.AddHostedService<PricingBackgroundService>()
	.AddGrpc();

var app = builder.Build();

app.MapGrpcService<PricingGrpcService>();
app.MapGet("/", () => "FeedR Quotes feed");

app.MapPost("/pricing/start", async (PricingRequestChannel channel) =>
{
	await channel.Requests.Writer.WriteAsync(new StartPricing());
	return Results.Ok();
});
app.MapPost("/pricing/stop", async (PricingRequestChannel channel) =>
{
	await channel.Requests.Writer.WriteAsync(new StopPricing());
	return Results.Ok();
});
app.Run();