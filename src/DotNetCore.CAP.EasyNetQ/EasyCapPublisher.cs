using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EasyCapPublisher : IEasyCapPublisher
    {
        private readonly ICapPublisher _publisher;

        public EasyCapPublisher(ICapPublisher publisher)
        {
            this._publisher = publisher;
        }

        public IServiceProvider ServiceProvider => _publisher.ServiceProvider;

        public AsyncLocal<ICapTransaction> Transaction => _publisher.Transaction;

        public void Publish<T>(string name, T contentObj, string callbackName = null)
        {
            _publisher.Publish(name, contentObj, callbackName);
        }

        public void Publish<T>(string name, T contentObj, IDictionary<string, string> headers)
        {
            _publisher.Publish(name, contentObj, headers);
        }

        public void Publish<T>(T contentObj, Type callbackMessage = null)
        {
            string name = AutoNamingStrategy.GetMessageName(typeof(T));
            string callbackName = null;
            if (callbackMessage != null)
                callbackName = AutoNamingStrategy.GetMessageName(callbackMessage);
            _publisher.Publish(name, contentObj, callbackName);
        }

        public void Publish<T>(T contentObj, IDictionary<string, string> headers)
        {
            string name = AutoNamingStrategy.GetMessageName(typeof(T));
            _publisher.Publish(name, contentObj, headers);
        }

        public Task PublishAsync<T>(string name, T contentObj, string callbackName = null, CancellationToken cancellationToken = default)
        {
            return _publisher.PublishAsync<T>(name, contentObj, callbackName, cancellationToken);
        }

        public Task PublishAsync<T>(string name, T contentObj, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            return _publisher.PublishAsync<T>(name, contentObj, headers, cancellationToken);
        }

        public Task PublishAsync<T>(T contentObj, Type callbackMessage = null, CancellationToken cancellationToken = default)
        {
            string name = AutoNamingStrategy.GetMessageName(typeof(T));
            string callbackName = null;
            if (callbackMessage != null)
                callbackName = AutoNamingStrategy.GetMessageName(callbackMessage);
            return _publisher.PublishAsync<T>(name, contentObj, callbackName, cancellationToken);
        }

        public Task PublishAsync<T>(T contentObj, IDictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            string name = AutoNamingStrategy.GetMessageName(typeof(T));
            return _publisher.PublishAsync<T>(name, contentObj, headers, cancellationToken);
        }
    }
}