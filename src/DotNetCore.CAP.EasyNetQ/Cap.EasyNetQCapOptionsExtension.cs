using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Transport;
using EasyNetQ;
using EasyNetQ.ConnectionString;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EasyNetQCapOptionsExtension : ICapOptionsExtension
    {
        private readonly Action<EasyNetQOptions> _configure;

        public EasyNetQCapOptionsExtension(Action<EasyNetQOptions> configure)
        {
            this._configure = configure;
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<CapMessageQueueMakerService>();
            services.Configure(_configure);

            services.AddSingleton<IEasyCapPublisher, EasyCapPublisher>();
            services.AddSingleton<ITransport, EasyNetQTransport>();
            services.AddSingleton<IConnectionStringParser, ConnectionStringParser>();
            services.AddSingleton<ITypeNameSerializer, LegacyTypeNameSerializer>();
            services.AddSingleton<IConnectionChannelPool, ConnectionChannelPool>();
            services.AddSingleton<IConsumerClientFactory, EasyNetQConsumerClientFactory>();
            services.AddSingleton<IConventions, Conventions>();
            services.Replace(ServiceDescriptor.Singleton<IConsumerServiceSelector, ConsumerServiceSelector>());

            ServiceProvider provider = services.BuildServiceProvider();
            AutoNamingStrategy.CreateAndActive(provider.GetService<IConventions>(), provider.GetService<IOptions<EasyNetQOptions>>());
        }
    }
}