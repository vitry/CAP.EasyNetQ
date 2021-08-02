using DotNetCore.CAP.Internal;
using System;
using System.Collections.Generic;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EasyNetQSubscribeAttribute : TopicAttribute
    {
        // name : exchang name
        // group : queue name
        public EasyNetQSubscribeAttribute(string queueName, string exchangeName, string subscriptionId = "", string[] topics = null)
            : base(exchangeName)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(nameof(queueName));
            if (string.IsNullOrEmpty(exchangeName))
                throw new ArgumentNullException(nameof(exchangeName));

            this.SubscriptionId = subscriptionId;
            this.QueueName = queueName;
            this.ExchangeName = exchangeName;
            this.Topics = topics ?? new string[] { };

            this.Group = this.QueueName;
        }

        public EasyNetQSubscribeAttribute(string queueName, string exchangeName, string subscriptionId = "", string topic = "#")
            : this(queueName, exchangeName, subscriptionId, new string[] { topic })
        {
        }

        public EasyNetQSubscribeAttribute(Type messageType, string subscriptionId = "", string topic = "#")
            : this(AutoNamingStrategy.GetQueueName(messageType, subscriptionId), AutoNamingStrategy.GetExchangeName(messageType), subscriptionId, topic)
        {
        }

        public string SubscriptionId { get; }

        public ushort ConsumerCount { get; set; } = 1;

        #region Queue

        public string QueueName { get; }

        public string ExchangeName { get; }

        public IEnumerable<string> Topics { get; }

        #endregion Queue

        #region SubscriptionConfiguration

        /// <summary>
        /// Delete the queue once all consumers have disconnected. (default false)
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        ///
        /// </summary>
        public int Priority { get; set; } = 0;

        private ushort _preFetchCount = 0;

        public ushort PrefetchCount
        {
            get { return _preFetchCount; }
            set
            {
                if (value > 0) _preFetchCount = value;
            }
        }

        /// <summary>
        /// How long in milliseconds the queue should remain unused before it is automatically deleted. (default not set
        /// </summary>
        public int Expires { get; set; }

        #endregion SubscriptionConfiguration

        private SubscriptionConfig _payload;

        internal void AppendSubscriptionConfig(Action<SubscriptionConfig> config)
        {
            this._payload ??= AsPayload();
            config?.Invoke(this._payload);
        }

        internal SubscriptionConfig GetSubscriptionConfig() => this._payload ??= AsPayload();

        private SubscriptionConfig AsPayload()
        {
            int? GetExpires(int expires)
            {
                if (expires == 0) return null;
                return expires;
            }

            return new SubscriptionConfig(this.QueueName, this.ExchangeName, this.SubscriptionId)
            {
                ConsumerCount = this.ConsumerCount,
                Topics = this.Topics,
                AutoDelete = this.AutoDelete,
                Priority = this.Priority,
                PrefetchCount = this.PrefetchCount,
                Expires = GetExpires(this.Expires),
            };
        }
    }
}