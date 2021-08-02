using System.Collections.Generic;
using System.Linq;

namespace DotNetCore.CAP.EasyNetQ
{
    public class SubscriptionConfig
    {
        public SubscriptionConfig(string queueName, string exchangeName, string subscriptionId = "")
        {
            this.SubscriptionId = subscriptionId;
            this.QueueName = queueName;
            this.ExchangeName = exchangeName;
        }

        public string SubscriptionId { get; }

        public ushort ConsumerCount { get; set; } = 1;

        public string QueueName { get; }

        public string ExchangeName { get; }

        private IEnumerable<string> _topics;

        public IEnumerable<string> Topics
        {
            get { return _topics; }
            set { _topics = value?.Where(p => !string.IsNullOrEmpty(p)).ToList() ?? new List<string>(); }
        }

        public ushort PrefetchCount { get; set; } = 0;

        /// <summary>
        /// Delete the queue once all consumers have disconnected. (default false)
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        ///
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// How long in milliseconds the queue should remain unused before it is automatically deleted. (default not set
        /// </summary>
        public int? Expires { get; set; } = null;

        /// <summary>
        /// Can only be accessed by the current connection. (default false)
        /// </summary>
        public bool IsExclusive { get; set; } = false;

        /// <summary>
        /// Determines the maximum message priority that the queue should support.
        /// </summary>
        public byte? MaxPriority { get; set; } = null;

        /// <summary>
        /// Can survive a server restart. If this is false the queue will be deleted when the server restarts. (default true)
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// The maximum number of ready messages that may exist on the queue.  Messages will be dropped or dead-lettered from the front of the queue to make room for new messages once the limit is reached.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// The maximum size of the queue in bytes.  Messages will be dropped or dead-lettered from the front of the queue to make room for new messages once the limit is reached
        /// </summary>
        public int? MaxLengthBytes { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string QueueMode { get; set; }
    }
}