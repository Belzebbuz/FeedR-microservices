using FeedR.Shared.Streaming;

namespace FeedR.Aggregator.Services;

public class PricingStreamBackgroundService : BackgroundService
{
	private readonly IStreamSubscriber _streamSubscriber;
	private readonly ILogger<PricingStreamBackgroundService> _logger;

	public PricingStreamBackgroundService(IStreamSubscriber streamSubscriber, ILogger<PricingStreamBackgroundService> logger)
	{
		_streamSubscriber = streamSubscriber;
		_logger = logger;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await _streamSubscriber.SubscribeAsync<CurrencyPair>("pricing", data =>
		{
			_logger.LogInformation($"Pricing: {data.Symbol} - {data.Value:F} , timestamp :{data.Timestamps}");
		});
	}

	private record CurrencyPair(string Symbol, decimal Value, long Timestamps);
}
