using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCore.CAP.EasyNetQ
{
    /// <inheritdoc />
    /// <summary>
    /// A default <see cref="T:DotNetCore.CAP.Abstractions.IConsumerServiceSelector" /> implementation.
    /// </summary>
    public class ConsumerServiceSelector : Internal.ConsumerServiceSelector, IConsumerServiceSelector
    {
        private readonly IOptions<CapOptions> _capOptions;
        private readonly IOptions<EasyNetQOptions> _options;

        /// <summary>
        /// Creates a new <see cref="ConsumerServiceSelector" />.
        /// </summary>
        public ConsumerServiceSelector(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _capOptions = serviceProvider.GetService<IOptions<CapOptions>>();
            _options = serviceProvider.GetService<IOptions<EasyNetQOptions>>();
        }

        public new IReadOnlyList<ConsumerExecutorDescriptor> SelectCandidates()
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();

            executorDescriptorList.AddRange(base.SelectCandidates());
            executorDescriptorList.AddRange(FindConsumersFromDelegate());
            executorDescriptorList = executorDescriptorList.Distinct(new ConsumerExecutorDescriptorComparer()).ToList();
            return executorDescriptorList;
        }

        protected IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromDelegate()
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();

            foreach (var info in _options.Value.Subscriptions)
            {
                var attr = new EasyNetQSubscribeAttribute(info.QueueName, info.ExchangeName, info.SubscriptionId, info.Topics);
                attr.AppendSubscriptionConfig(info.Config);
                base.SetSubscribeAttribute(attr);

                var methodInfo = info.HandleMethod;
                var typeInfo = methodInfo.DeclaringType.GetTypeInfo();
                var parameters = methodInfo.GetParameters().Select(parameter => new ParameterDescriptor
                {
                    Name = parameter.Name,
                    ParameterType = parameter.ParameterType,
                    IsFromCap = parameter.GetCustomAttributes(typeof(FromCapAttribute)).Any()
                }).ToList();

                yield return InitDescriptor(attr, methodInfo, typeInfo, null, parameters, null);
            }
        }

        private ConsumerExecutorDescriptor InitDescriptor(
            TopicAttribute attr,
            MethodInfo methodInfo,
            TypeInfo implType,
            TypeInfo serviceTypeInfo,
            IList<ParameterDescriptor> parameters,
            TopicAttribute classAttr = null)
        {
            var descriptor = new ConsumerExecutorDescriptor
            {
                Attribute = attr,
                ClassAttribute = classAttr,
                MethodInfo = methodInfo,
                ImplTypeInfo = implType,
                ServiceTypeInfo = serviceTypeInfo,
                Parameters = parameters,
                TopicNamePrefix = _capOptions.Value.TopicNamePrefix
            };

            return descriptor;
        }
    }
}