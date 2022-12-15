using FeedR.Shared.Serialization;
using FeedR.Shared.Streaming;
using StackExchange.Redis;

namespace FeedR.Shared.Redis.Streaming;

public class RedisStreamSubscriber : IStreamSubscriber
{
	private readonly ISerializer _serializer;
	private readonly ISubscriber _subscriber;

	public RedisStreamSubscriber(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer)
	{
		_subscriber = connectionMultiplexer.GetSubscriber();
		_serializer = serializer;
	}
	public Task SubscribeAsync<T>(string topic, Action<T> handler) where T : class
		=> _subscriber.SubscribeAsync(topic, (_, data) =>
		{
			var payload = _serializer.Deserialize<T>(data);
			if(payload is null)
			{
				return;
			}
			handler(payload);
		});
}
