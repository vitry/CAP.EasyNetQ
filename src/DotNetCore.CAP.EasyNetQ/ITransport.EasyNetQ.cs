using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Transport;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EasyNetQTransport : ITransport
    {
        private readonly ILogger<EasyNetQTransport> _logger;
        private readonly IConnectionChannelPool _connectionChannelPool;
        private readonly IBus _bus;

        public EasyNetQTransport(
            ILogger<EasyNetQTransport> logger,
            IConnectionChannelPool connectionChannelPool)
        {
            this._logger = logger;
            this._connectionChannelPool = connectionChannelPool;
            this._bus = this._connectionChannelPool.Bus;
        }

        public BrokerAddress BrokerAddress => new BrokerAddress("EasyNetQ",
            _connectionChannelPool.HostAddress);

        public async Task<OperateResult> SendAsync(TransportMessage message)
        {
            try
            {
                // todo: add cancel token
                var exchange = await _bus.Advanced.ExchangeDeclareAsync(
                    message.GetName(), message.GetExchangeType()).ConfigureAwait(false);

                var properties = new MessageProperties();
                if (message.Headers.ContainsKey(EasyNetQHeaders.PRIORITY))
                    properties.Priority = message.GetPriority();
                if (message.Headers.ContainsKey(EasyNetQHeaders.EXPIRE))
                    properties.Expiration = message.GetExpire().ToString();
                properties.DeliveryMode = MessageDeliveryMode.Persistent;
                properties.Headers = message.GetHeaders();

                // todo: add cancel token
                await _bus.Advanced.PublishAsync(exchange, message.GetRoute(), false, properties, message.Body)
                    .ConfigureAwait(false);

                _logger.LogDebug($"EasyNetQ message [{message.GetName()}] {message.GetId()} has been published.");
                return OperateResult.Success;
            }
            catch (System.Exception ex)
            {
                var wrapperEx = new PublisherSentFailedException(ex.Message, ex);
                var errors = new OperateError
                {
                    Code = ex.HResult.ToString(),
                    Description = ex.Message
                };

                return OperateResult.Failed(wrapperEx, errors);
            }
        }
    }
}