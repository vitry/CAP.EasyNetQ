using DotNetCore.CAP.Messages;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCore.CAP.EasyNetQ
{
    public static class EasyNetQHeaders
    {
        public const string TOPIC = "easynetq-topic";
        public const string PRIORITY = "easynetq-priority";
        public const string EXPIRE = "easynetq-expire";
        public const string TYPE = "easynetq-type";
    }

    public static class TransportMessageExtensions
    {
        public static string GetTopic(this TransportMessage message)
        {
            return message.Headers.TryGetValue(EasyNetQHeaders.TOPIC, out var value) ? value : "";
        }

        public static byte GetPriority(this TransportMessage message)
        {
            return MessagePriority.ConvertToPriority(message.Headers[EasyNetQHeaders.PRIORITY]);
        }

        public static int GetExpire(this TransportMessage message)
        {
            return int.Parse(message.Headers[EasyNetQHeaders.EXPIRE]);
        }

        public static IDictionary<string, object> GetHeaders(this TransportMessage message)
        {
            message.Headers.Remove(EasyNetQHeaders.TOPIC);
            message.Headers.Remove(EasyNetQHeaders.PRIORITY);
            message.Headers.Remove(EasyNetQHeaders.EXPIRE);

            return message.Headers.ToDictionary(p => p.Key, p => (object)p.Value);
        }
    }
}