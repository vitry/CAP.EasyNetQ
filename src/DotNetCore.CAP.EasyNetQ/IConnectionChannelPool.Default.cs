using EasyNetQ;
using EasyNetQ.ConnectionString;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace DotNetCore.CAP.EasyNetQ
{
    public class ConnectionChannelPool : IConnectionChannelPool
    {
        private readonly ILogger<ConnectionChannelPool> _logger;
        private readonly IConnectionStringParser _connectionParser;
        private readonly IBus _bus;

        public ConnectionChannelPool(
            ILogger<ConnectionChannelPool> logger,
            IConnectionStringParser connectionParser,
            IOptions<EasyNetQOptions> options)
        {
            this._logger = logger;
            this._connectionParser = connectionParser;

            ConnectionConfiguration = _connectionParser.Parse(options.Value.Connection);
            this._bus = RabbitHutch.CreateBus(ConnectionConfiguration, p => { });
            _logger.LogInformation("EasyNetQ connected");
        }

        public ConnectionConfiguration ConnectionConfiguration { get; }

        public IBus Bus => _bus;

        public string HostAddress => string.Join('|', ConnectionConfiguration.Hosts.Select(p => p.Host));
    }
}