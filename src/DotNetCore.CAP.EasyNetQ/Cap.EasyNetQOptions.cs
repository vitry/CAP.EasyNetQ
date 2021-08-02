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
            string subscriptionId, Action<TMessage, THeaders> subscribe,
            Action<SubscriptionConfig> config = null)
            where TMessage : class
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));
            AddSubscription(subscriptionId, typeof(TMessage), subscribe.GetMethodInfo(), config);
        }

        public void Subscribe<TMessage, THeaders>(
            string subscriptionId, Func<TMessage, THeaders, Task> subscribe,
            Action<SubscriptionConfig> config = null)
            where TMessage : class
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));
            AddSubscription(subscriptionId, typeof(TMessage), subscribe.GetMethodInfo(), config);
        }

        private void AddSubscription(string subscriptionId, Type messageType, MethodInfo onMessage,
            Action<SubscriptionConfig> config = null)
        {
            if (string.IsNullOrEmpty(subscriptionId)) subscriptionId = this.SubscriptionId;
            Subscriptions.Add(new SubscriptionInfo(messageType, subscriptionId)
            {
                HandleMethod = onMessage,
                Config = config
            });
        }
    }

    public class SubscriptionInfo
    {
        public SubscriptionInfo(Type type, string subscriptionId)
        {
            this.SubscriptionId = subscriptionId;
            this.QueueName = AutoNamingStrategy.GetQueueName(type, subscriptionId);
            this.ExchangeName = AutoNamingStrategy.GetExchangeName(type);
        }

        public string QueueName { get; }

        public string ExchangeName { get; }

        public string SubscriptionId { get; }

        public string[] Topics { get; set; }

        public MethodInfo HandleMethod { get; set; }

        public Action<SubscriptionConfig> Config { get; set; }
    }
}