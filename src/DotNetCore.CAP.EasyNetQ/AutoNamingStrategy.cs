using EasyNetQ;
using Microsoft.Extensions.Options;
using System;

namespace DotNetCore.CAP.EasyNetQ
{
    public static class AutoNamingStrategy
    {
        private static IConventions _conventions;
        private static IOptions<EasyNetQOptions> _options;

        public static void CreateAndActive(IConventions conventions,
            IOptions<EasyNetQOptions> options)
        {
            if (conventions == null)
                throw new ArgumentNullException(nameof(conventions));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _conventions = conventions;
            _options = options;
        }

        public static string GetQueueName(Type messageType, string subscriptionId = "")
        {
            if (_conventions == null) throw new ArgumentNullException(nameof(_conventions));
            if (string.IsNullOrEmpty(subscriptionId)) subscriptionId = _options.Value.SubscriptionId;
            return _conventions.QueueNamingConvention(messageType, subscriptionId);
        }

        public static string GetMessageName(Type messageType)
        {
            if (_conventions == null) throw new ArgumentNullException(nameof(_conventions));
            return _conventions.ExchangeNamingConvention(messageType);
        }

        public static string GetExchangeName(Type messageType)
        {
            if (_conventions == null) throw new ArgumentNullException(nameof(_conventions));
            string messageName = GetMessageName(messageType);
            return messageName;
        }
    }
}