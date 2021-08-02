using EasyNetQ;
using System;
using System.Collections.Generic;

namespace DotNetCore.CAP.EasyNetQ
{
    public class Subscription
    {
        public Subscription(string queue, string exchange,
            IEnumerable<string> topics, ushort consumerCount,
            Action<IQueueDeclareConfiguration> queueSetting,
            Action<IConsumerConfiguration> consumerSetting)
        {
            Queue = queue;
            Exchange = exchange;
            Topics = topics;
            ConsumerCount = consumerCount;
            QueueSetting = queueSetting;
            ConsumerSetting = consumerSetting;
        }

        public string Queue { get; }

        public string Exchange { get; }

        public IEnumerable<string> Topics { get; }

        public ushort ConsumerCount { get; }

        public Action<IQueueDeclareConfiguration> QueueSetting { get; }

        public Action<IConsumerConfiguration> ConsumerSetting { get; }
    }
}