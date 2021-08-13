using EasyNetQ.Topology;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCore.CAP.Messages
{
    public static class EasyNetQHeaders
    {
        /// <summary>
        /// 消息优先级
        /// </summary>
        public const string PRIORITY = "easynetq-priority";

        /// <summary>
        /// 过期时间
        /// </summary>
        public const string EXPIRE = "easynetq-expire";

        /// <summary>
        /// 消息类型
        /// </summary>
        public const string TYPE = "easynetq-type";

        /// <summary>
        /// 消息交换器模式
        /// </summary>
        public const string EXCHANGE_TYPE = "easynetq-exchange-type";

        /// <summary>
        /// 消息路由键
        /// </summary>
        public const string ROUTE = "easynetq-route";
    }

    public static class TransportMessageExtensions
    {
        public static string GetExchangeType(this TransportMessage message)
        {
            string exchangeType = message.Headers.TryGetValue(EasyNetQHeaders.EXCHANGE_TYPE, out var value) ? value : ExchangeType.Topic;
            if (string.IsNullOrEmpty(exchangeType)) exchangeType = ExchangeType.Direct;
            return exchangeType;
        }

        public static string GetRoute(this TransportMessage message)
        {
            return message.Headers.TryGetValue(EasyNetQHeaders.ROUTE, out var value) ? value : "";
        }

        public static byte GetPriority(this TransportMessage message)
        {
            return EasyNetQ.MessagePriority.ConvertToPriority(message.Headers[EasyNetQHeaders.PRIORITY]);
        }

        public static int GetExpire(this TransportMessage message)
        {
            return int.Parse(message.Headers[EasyNetQHeaders.EXPIRE]);
        }

        public static IDictionary<string, object> GetHeaders(this TransportMessage message)
        {
            return message.Headers.ToDictionary(p => p.Key, p => (object)p.Value);
        }
    }
}
