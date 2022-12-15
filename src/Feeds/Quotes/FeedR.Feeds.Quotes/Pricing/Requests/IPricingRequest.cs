namespace FeedR.Feeds.Quotes.Pricing.Requests;

internal interface IPricingRequest
{
}

internal record StartPricing : IPricingRequest;
internal record StopPricing : IPricingRequest;