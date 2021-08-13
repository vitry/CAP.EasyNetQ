using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Internals;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.CAP.EasyNetQ
{
    internal sealed class EasyNetQConsumerClient : IConsumerClient
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly string _groupId;
        private readonly IConnectionChannelPool _connectionChannelPool;
        private readonly IOptions<EasyNetQOptions> _options;
        private readonly IBus _bus;

        private readonly MethodMatcherCache _selector;
        private readonly ConnectionConfiguration _connectionConfig;
        private List<Subscription> _subscriptions;
        private List<IDisposable> _consumers;
        private ConcurrentDictionary<string, AckStrategy> _ackResults;
        private ILogger<EasyNetQConsumerClient> _logger;

        public EasyNetQConsumerClient(
            ILoggerFactory loggerFactory,
            string groupId,
            IConnectionChannelPool connectionChannelPool,
            IOptions<EasyNetQOptions> options,
            MethodMatcherCache selector)
        {
            this._loggerFactory = loggerFactory;
            this._groupId = groupId;
            this._connectionChannelPool = connectionChannelPool;
            this._options = options;
            this._selector = selector;

            this._bus = this._connectionChannelPool.Bus;
            this._connectionConfig = this._connectionChannelPool.ConnectionConfiguration;
            this._logger = this._loggerFactory.CreateLogger<EasyNetQConsumerClient>();
            this._subscriptions = new List<Subscription>();
            this._consumers = new List<IDisposable>();
            this._ackResults = new ConcurrentDictionary<string, AckStrategy>();
        }

        public BrokerAddress BrokerAddress => new BrokerAddress("EasyNetQ",
            _connectionChannelPool.HostAddress);

        public event EventHandler<TransportMessage> OnMessageReceived;

        public event EventHandler<LogMessageEventArgs> OnLog;

        public void Connect(MessageHandler onMessage, CancellationToken cancellationToken)
        {
            using var cts = cancellationToken.WithTimeout(_connectionConfig.Timeout);

            foreach (var subscription in _subscriptions)
            {
                var queue = _bus.Advanced.QueueDeclare(
                    subscription.Queue, subscription.QueueSetting, cts.Token);
                var exchange = _bus.Advanced.ExchangeDeclare(
                    subscription.Exchange, subscription.ExchangeType, cancellationToken: cts.Token);
                foreach (var topic in subscription.Routes)
                    _bus.Advanced.Bind(exchange, queue, topic, cts.Token);

                IEnumerable<IDisposable> consumers =
                    Enumerable.Range(0, subscription.ConsumerCount).Select(
                        _ => _bus.Advanced.Consume(queue, onMessage, subscription.ConsumerSetting));
                _consumers.AddRange(consumers);
            }
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            MessageHandler onMessage = (body, properties, info, token) =>
                Task.Factory.StartNew(() =>
                {
                    var headers = new Dictionary<string, string>();

                    // only for cap compatible
                    headers.Add(Headers.Group, _groupId);

                    if (properties.Headers != null)
                    {
                        foreach (var header in properties.Headers)
                            headers.Add(header.Key, header.Value == null ? null : Encoding.UTF8.GetString((byte[])header.Value));
                    }
                    // append easynetq type info
                    if (properties.TypePresent) headers.Add(EasyNetQHeaders.TYPE, properties.Type);

                    // subscribe by exchange and queue name
                    if (!headers.ContainsKey(Headers.MessageName))
                    {
                        Subscription subscription = _subscriptions.FirstOrDefault(p => p.Queue == info.Queue)
                            ?? throw new NullReferenceException($"Queue:{info.Queue} does not exits subscriber.");
                        headers.Add(Headers.MessageName, subscription.Exchange);
                    }

                    if (_options.Value.CapHeaderMaps.Count > 0)
                        using (var doc = System.Text.Json.JsonDocument.Parse(body))
                        {
                            foreach (var map in _options.Value.CapHeaderMaps)
                            {
                                if (headers.ContainsKey(map.Key)) continue;

                                var elementSearchingRoad = map.Value.Split('.');
                                var element = doc.RootElement;
                                for (int i = 0; i < elementSearchingRoad.Length; i++)
                                    element = element.GetProperty(elementSearchingRoad[i]);
                                headers.Add(map.Key, element.GetString());
                            }
                        }
                    var message = new TransportMessage(headers, body);

                    string ackKey = $"{info.Queue}.{info.DeliveryTag}";
                    _logger.LogAckKeyCreated(ackKey);

                    var tcs = new TaskCompletionSource<bool>();
                    var syncCtx = new EventHandleSynchronizationContext(() => tcs.SetResult(true));
                    SynchronizationContext.SetSynchronizationContext(syncCtx);

                    OnMessageReceived?.Invoke(ackKey, message);
                    tcs.Task.GetAwaiter().GetResult();

                    if (!_ackResults.TryRemove(ackKey, out AckStrategy ackResult))
                        _logger.LogMissingAck(ackKey);
                    else
                        _logger.LogAckKeyDisposed(ackKey);
                    return ackResult;
                });

            Connect(onMessage, cancellationToken);
        }

        public void Commit(object sender)
        {
            _ackResults.AddOrUpdate((string)sender, AckStrategies.Ack,
                (deliveryTag, ackResult) => AckStrategies.Ack);
            _logger.LogAckResultAdded((string)sender);
        }

        public void Reject(object sender)
        {
            _ackResults.AddOrUpdate((string)sender, AckStrategies.NackWithRequeue,
                (deliveryTag, ackResult) => AckStrategies.NackWithRequeue);
            _logger.LogNackResultAdded((string)sender);
        }

        public void Dispose()
        {
            // do nothings
        }

        public void Subscribe(IEnumerable<string> subscriptionNames)
        {
            if (subscriptionNames == null)
                throw new ArgumentNullException(nameof(subscriptionNames));

            foreach (var subscriptionName in subscriptionNames)
            {
                _selector.TryGetTopicExecutor(subscriptionName, _groupId, out ConsumerExecutorDescriptor descriptor);
                var subscription = descriptor.Attribute as EasyNetQSubscribeAttribute;
                if (subscription == null) continue;

                var subscriptionConfig = subscription.GetSubscriptionConfig();
                if (subscriptionConfig.PrefetchCount == 0)
                    subscriptionConfig.PrefetchCount = _connectionChannelPool.ConnectionConfiguration.PrefetchCount;

                Action<IQueueDeclareConfiguration> queueSetting = c =>
                {
                    c.AsDurable(subscriptionConfig.Durable);
                    c.AsAutoDelete(subscriptionConfig.AutoDelete);
                    if (subscriptionConfig.Expires.HasValue)
                        c.WithExpires(TimeSpan.FromMilliseconds(subscriptionConfig.Expires.Value));
                    if (subscriptionConfig.MaxPriority.HasValue)
                        c.WithMaxPriority(subscriptionConfig.MaxPriority.Value);
                    if (subscriptionConfig.MaxLength.HasValue)
                        c.WithMaxLength(subscriptionConfig.MaxLength.Value);
                    if (subscriptionConfig.MaxLengthBytes.HasValue)
                        c.WithMaxLengthBytes(subscriptionConfig.MaxLengthBytes.Value);
                    if (!string.IsNullOrEmpty(subscriptionConfig.QueueMode))
                        c.WithQueueMode(subscriptionConfig.QueueMode);
                };
                Action<IConsumerConfiguration> consumerSetting = c =>
                    c.WithPrefetchCount(subscriptionConfig.PrefetchCount)
                        .WithPriority(subscriptionConfig.Priority)
                        .WithExclusive(subscriptionConfig.IsExclusive);

                _subscriptions.Add(new Subscription(subscriptionConfig.QueueName, subscriptionConfig.ExchangeName,
                    subscriptionConfig.ExchangeType, subscriptionConfig.Routes,
                    subscriptionConfig.ConsumerCount, queueSetting, consumerSetting));
            }
        }
    }

    public static class EasyNetQLogerExtensions
    {
        public static void LogMissingAck(this ILogger logger, string ackKey)
        {
            logger.LogError($"EasyNetQ consume can not ack because of missing ackKey '{ackKey}' in thread {Thread.CurrentThread.ManagedThreadId}");
        }

        public static void LogAckKeyDisposed(this ILogger logger, string ackKey)
        {
            logger.LogDebug($"EasyNetQ consume client disposed ackKey: '{ackKey}' in thread {Thread.CurrentThread.ManagedThreadId}");
        }

        public static void LogAckKeyCreated(this ILogger logger, string ackKey)
        {
            logger.LogDebug($"EasyNetQ consume client created ackKey: '{ackKey}' in thread {Thread.CurrentThread.ManagedThreadId}");
        }

        public static void LogAckResultAdded(this ILogger logger, string ackKey)
        {
            logger.LogDebug($"EasyNetQ consume client ack added: '{ackKey}' in thread {Thread.CurrentThread.ManagedThreadId}");
        }

        public static void LogNackResultAdded(this ILogger logger, string ackKey)
        {
            logger.LogDebug($"EasyNetQ consume client nack added: '{ackKey}' in thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}