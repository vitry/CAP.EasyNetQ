using System.Collections.Generic;
using ExchangeType = DotNetCore.CAP.EasyNetQ.ExchangeType;

namespace DotNetCore.CAP.With
{
    public static class EasyNetQ
    {
        public static IDictionary<string, string> Direct(string routingKey)
        {
            return new Dictionary<string, string>
            {
                { Messages.EasyNetQHeaders.EXCHANGE_TYPE, ExchangeType.DIRECT },
                { Messages.EasyNetQHeaders.ROUTE, routingKey }
            };
        }

        public static IDictionary<string, string> Topic(string topic)
        {
            return new Dictionary<string, string>
            {
                { Messages.EasyNetQHeaders.EXCHANGE_TYPE, ExchangeType.TOPIC },
                { Messages.EasyNetQHeaders.ROUTE, topic }
            };
        }
    }
}