using DotNetCore.CAP.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EasyNetQSubscribeAttribute : TopicAttribute
    {
        // name : exchang name
        // group : queue name
        public EasyNetQSubscribeAttribute(string queueName, string exchangeName, string subscriptionId,
            string[] routes, string exchangeType)
            : base(exchangeName)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(nameof(queueName));
            if (string.IsNullOrEmpty(exchangeName))
                throw new ArgumentNullException(nameof(exchangeName));

            this.SubscriptionId = subscriptionId;
            this.QueueName = queueName;
            this.ExchangeName = exchangeName;
            this.Routes = routes?.Where(p => !string.IsNullOrEmpty(p)) ?? new string[] { };
            this.ExchangeType = exchangeType;
            this.Group = this.QueueName;
        }

        public EasyNetQSubscribeAttribute(string queueName, string exchangeName, string subscriptionId = "",
            string route = "#", string exchangeType = EasyNetQ.ExchangeType.TOPIC)
            : this(queueName, exchangeName, subscriptionId, new string[] { route }, exchangeType)
        {
        }

        public EasyNetQSubscribeAttribute(Type messageType, string subscriptionId = "", 
            string route = "#", string exchangeType = EasyNetQ.ExchangeType.TOPIC)
            : this(AutoNamingStrategy.GetQueueName(messageType, subscriptionId),
                  AutoNamingStrategy.GetExchangeName(messageType), subscriptionId, route, exchangeType)
        {
        }

        public string SubscriptionId { get; }

        public ushort ConsumerCount { get; set; } = 1;

        #region Queue

        public string QueueName { get; }

        public string ExchangeName { get; }

        public IEnumerable<string> Routes { get; }

        public string ExchangeType { get; }

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

            return new SubscriptionConfig(this.QueueName, this.ExchangeName, this.SubscriptionId, this.ExchangeType)
            {
                ConsumerCount = this.ConsumerCount,
                Routes = this.Routes,
                AutoDelete = this.AutoDelete,
                Priority = this.Priority,
                PrefetchCount = this.PrefetchCount,
                Expires = GetExpires(this.Expires),
            };
        }
    }
}