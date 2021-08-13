using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.CAP.EasyNetQ
{
    public interface IEasyCapPublisher : ICapPublisher
    {
        void Publish<T>(T contentObj, Type callbackMessage = null);

        void Publish<T>(T contentObj, IDictionary<string, string> headers);

        Task PublishAsync<T>(T contentObj, Type callbackMessage = null, CancellationToken cancellationToken = default);

        Task PublishAsync<T>(T contentObj, IDictionary<string, string> headers, CancellationToken cancellationToken = default);
    }
}