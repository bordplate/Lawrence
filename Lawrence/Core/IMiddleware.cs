using System;

namespace Lawrence.Core;

public interface IMiddleware {
    public void Execute(PacketContext context, Action<PacketContext> next);
}