using System.Threading.Channels;
using ECommerce.API.Infrastructure.Database.Entities;

namespace ECommerce.API.Infrastructure.Logging;

public static class LogQueue
{
    // Bellekte tutulan yüksek performanslı log kuyruğu. (Bounded yapılıp bellek taşması engellenebilir)
    public static Channel<SystemLog> Channel { get; } = System.Threading.Channels.Channel.CreateUnbounded<SystemLog>();
}
