using Topology = EasyNetQ.Topology;

namespace DotNetCore.CAP.EasyNetQ
{
    /// <summary>
    /// RabbitMQ Exchange Type
    /// </summary>
    public static class ExchangeType
    {
        /// <summary>
        /// rabbitmq exchange type: direct
        /// </summary>
        public const string DIRECT = Topology.ExchangeType.Direct;

        /// <summary>
        /// rabbitmq exchange type: topic
        /// </summary>
        public const string TOPIC = Topology.ExchangeType.Topic;

        /// <summary>
        /// rabbitmq exchange type: fanout
        /// </summary>
        public const string FANOUT = Topology.ExchangeType.Fanout;

        /// <summary>
        /// rabbitmq exchange type: header
        /// </summary>
        public const string HEADER = Topology.ExchangeType.Header;
    }
}