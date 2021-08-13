using EasyNetQ;
using System;
using System.Collections.Generic;

namespace DotNetCore.CAP.EasyNetQ
{
    public class Subscription
    {
        public Subscription(string queue, string exchange, string exchangeType,
            IEnumerable<string> routes, ushort consumerCount,
            Action<IQueueDeclareConfiguration> queueSetting,
            Action<IConsumerConfiguration> consumerSetting)
        {
            Queue = queue;
            Exchange = exchange;
            ExchangeType = exchangeType;
            Routes = routes;
            ConsumerCount = consumerCount;
            QueueSetting = queueSetting;
            ConsumerSetting = consumerSetting;
        }

        public string Queue { get; }

        public string Exchange { get; }

        public string ExchangeType { get; }

        public IEnumerable<string> Routes { get; }

        public ushort ConsumerCount { get; }

        public Action<IQueueDeclareConfiguration> QueueSetting { get; }

        public Action<IConsumerConfiguration> ConsumerSetting { get; }
    }
}