using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EasyNetQOptions
    {
        public EasyNetQOptions()
        {
            Subscriptions = new List<SubscriptionInfo>();
        }

        /// <summary>
        /// easynetq default mq connections
        /// </summary>
        public const string DEFAULT_CONNECTION = "host=localhost";

        /// <summary>
        /// easynetq default subcriber's name
        /// </summary>
        public const string DEFAULT_SUBSCRIPTION_ID = "Default";

        public string Connection { get; set; } = DEFAULT_CONNECTION;

        public string SubscriptionId { get; set; } = DEFAULT_SUBSCRIPTION_ID;

        public Dictionary<string, string> CapHeaderMaps { get; set; } = new Dictionary<string, string>();

        public IList<SubscriptionInfo> Subscriptions { get; private set; }

        public void Subscribe<TMessage, THeaders>(
            string subscriptionId, Action<TMessage, THeaders> subscribe, string exchangeName, string queueName,
            Action<SubscriptionConfig> setConfigs = null, string exchangeType = ExchangeType.TOPIC)
            where THeaders : class
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));
            AddSubscription(subscriptionId, exchangeName, queueName, subscribe.GetMethodInfo(), setConfigs, exchangeType);
        }

        public void Subscribe<TMessage, THeaders>(
            string subscriptionId, Action<TMessage, THeaders> subscribe,
            Action<SubscriptionConfig> setConfigs = null, string exchangeType = ExchangeType.TOPIC)
            where TMessage : class
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));
            Type messageType = typeof(TMessage);
            AddSubscription(subscriptionId, AutoNamingStrategy.GetExchangeName(messageType),
                AutoNamingStrategy.GetQueueName(messageType, subscriptionId), subscribe.GetMethodInfo(), setConfigs, exchangeType);
        }

        public void Subscribe<TMessage, THeaders>(
            string subscriptionId, Func<TMessage, THeaders, Task> subscribe, string exchangeName, string queueName,
            Action<SubscriptionConfig> setConfigs = null, string exchangeType = ExchangeType.TOPIC)
            where THeaders : class
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));
            AddSubscription(subscriptionId, exchangeName, queueName, subscribe.GetMethodInfo(), setConfigs, exchangeType);
        }

        public void Subscribe<TMessage, THeaders>(
            string subscriptionId, Func<TMessage, THeaders, Task> subscribe,
            Action<SubscriptionConfig> setConfigs = null, string exchangeType = ExchangeType.TOPIC)
            where TMessage : class
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));
            Type messageType = typeof(TMessage);
            AddSubscription(subscriptionId, AutoNamingStrategy.GetExchangeName(messageType),
                AutoNamingStrategy.GetQueueName(messageType, subscriptionId), subscribe.GetMethodInfo(), setConfigs, exchangeType);
        }

        private void AddSubscription(string subscriptionId, string exchangeName, string queueName, MethodInfo onMessage,
            Action<SubscriptionConfig> setConfigs = null, string exchangeType = ExchangeType.TOPIC)
        {
            if (string.IsNullOrEmpty(subscriptionId)) subscriptionId = this.SubscriptionId;
            Subscriptions.Add(
                new SubscriptionInfo(exchangeName, queueName, subscriptionId, exchangeType)
                {
                    HandleMethod = onMessage,
                    SetConfigs = setConfigs
                });
        }
    }

    public class SubscriptionInfo
    {
        public SubscriptionInfo(string exchangeName, string queueName, string subscriptionId, string exchangeType)
        {
            this.SubscriptionId = subscriptionId;
            this.ExchangeType = exchangeType;
            this.QueueName = queueName;
            this.ExchangeName = exchangeName;
        }

        public string QueueName { get; }

        public string ExchangeName { get; }

        public string SubscriptionId { get; }

        public string ExchangeType { get; }

        public string[] Routes { get; set; }

        public MethodInfo HandleMethod { get; set; }

        public Action<SubscriptionConfig> SetConfigs { get; set; }
    }
}