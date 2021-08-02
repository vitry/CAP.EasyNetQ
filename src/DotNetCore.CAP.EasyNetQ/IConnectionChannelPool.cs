using EasyNetQ;

namespace DotNetCore.CAP.EasyNetQ
{
    public interface IConnectionChannelPool
    {
        string HostAddress { get; }

        ConnectionConfiguration ConnectionConfiguration { get; }

        IBus Bus { get; }
    }
}