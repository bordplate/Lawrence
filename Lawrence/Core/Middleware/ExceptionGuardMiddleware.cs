using System;

namespace Lawrence.Core.Middleware;

public class ExceptionGuardMiddleware : IMiddleware {
    public void Execute(PacketContext context, Action<PacketContext> next) {
        try {
            next(context);
        } catch (Exception e) {
            Logger.Trace($"{context.Client.GetUsername()} threw an exception:", e);

            if (context.Client.IsDisconnected() && context.Client.IsActive()) {
                context.Client.ShowErrorMessage($"Server encountered an error:\n{e.Message}");
                context.Client.Disconnect();
            }
        }
    }
}