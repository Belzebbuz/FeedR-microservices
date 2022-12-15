using FeedR.Feeds.Quotes.Pricing.Requests;
using FeedR.Shared.Streaming;

namespace FeedR.Feeds.Quotes.Pricing.Services;

internal class PricingBackgroundService : BackgroundService
{
	private int _runningStatus;
	private readonly IPricingGenerator _pricingGenerator;
	private readonly PricingRequestChannel _requestChannel;
	private readonly ILogger<PricingBackgroundService> _logger;
	private readonly IStreamPublisher _streamPublisher;

	public PricingBackgroundService(IPricingGenerator pricingGenerator,
		PricingRequestChannel requestChanel, 
		ILogger<PricingBackgroundService> logger,
		IStreamPublisher streamPublisher)
	{
		_pricingGenerator = pricingGenerator;
		_requestChannel = requestChanel;
		_logger = logger;
		_streamPublisher = streamPublisher;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await foreach (var request in _requestChannel.Requests.Reader.ReadAllAsync(stoppingToken))
		{
			var _ = request switch
			{
				StartPricing => StartGeneratorAsync(),
				StopPricing => StopGeneratorAsync(),
				_ => Task.CompletedTask
			};
		}
	}

	private async Task StartGeneratorAsync()
	{
		if(Interlocked.Exchange(ref _runningStatus, 1) == 1)
		{
			return;
		}
		await foreach (var currencyPair in _pricingGenerator.StartAsync())
		{
			_logger.LogInformation($"Publishing the currency pair...");
			await _streamPublisher.PublishAsync("pricing", currencyPair);
		}
	}

	private async Task StopGeneratorAsync()
	{
		if (Interlocked.Exchange(ref _runningStatus, 0) == 0)
		{
			return;
		}

		await _pricingGenerator.StopAsync();
	}
}
