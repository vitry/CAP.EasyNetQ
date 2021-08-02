using DotNetCore.CAP;
using DotNetCore.CAP.EasyNetQ;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CapOptionsExtensions
    {
        public static CapOptions UseEasyNetQ(this CapOptions options)
        {
            return options.UseEasyNetQ(_ => { });
        }

        public static CapOptions UseEasyNetQ(this CapOptions options, string subscriptionId)
        {
            return options.UseEasyNetQ(p => p.SubscriptionId = subscriptionId);
        }

        public static CapOptions UseEasyNetQ(this CapOptions options, string connectString, string subscriptionId)
        {
            return options.UseEasyNetQ(
                p => { p.Connection = connectString; p.SubscriptionId = subscriptionId; });
        }

        public static CapOptions UseEasyNetQ(this CapOptions options, Action<EasyNetQOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            if (!string.IsNullOrEmpty(options.TopicNamePrefix))
                throw new NotSupportedException($"Cap TopicNamePrefix is not supported by EasyNetQ transport.");

            options.RegisterExtension(new EasyNetQCapOptionsExtension(configure));
            return options;
        }
    }
}