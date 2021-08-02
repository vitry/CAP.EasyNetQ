using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace DotNetCore.CAP.EasyNetQ
{
    internal sealed class EasyNetQConsumerClientFactory : IConsumerClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IConnectionChannelPool _connectionChannelPool;
        private readonly IOptions<EasyNetQOptions> _options;
        private readonly MethodMatcherCache _selector;

        public EasyNetQConsumerClientFactory(
            ILoggerFactory loggerFactory,
            IConnectionChannelPool connectionChannelPool,
            IOptions<EasyNetQOptions> options,
            MethodMatcherCache selector
            )
        {
            this._loggerFactory = loggerFactory;
            this._logger = this._loggerFactory.CreateLogger<EasyNetQConsumerClientFactory>();
            this._connectionChannelPool = connectionChannelPool;
            this._options = options;
            this._selector = selector;
        }

        public IConsumerClient Create(string groupId)
        {
            try
            {
                var client = new EasyNetQConsumerClient(
                    _loggerFactory, groupId, _connectionChannelPool,
                    _options, _selector);
                _logger.LogInformation($"EasyNetQ consumer id:{client.GetHashCode()} created.");
                return client;
            }
            catch (Exception e)
            {
                throw new BrokerConnectionException(e);
            }
        }
    }
}