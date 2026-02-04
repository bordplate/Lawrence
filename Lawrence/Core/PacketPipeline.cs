using System;
using System.Collections.Generic;

namespace Lawrence.Core;

public class PacketPipeline(List<IMiddleware> middlewares) {
    private Dictionary<Action<PacketContext>, Action<PacketContext>> _cachedPipelines = new();
    
    private Action<PacketContext, Action<PacketContext>> terminal = (ctx, last) => {
        last(ctx);
    };

    public Action<PacketContext> Get(Action<PacketContext> func) {
        if (_cachedPipelines.TryGetValue(func, out var cached)) {
            return cached;
        }

        var pipeline = Build(func);
        
        _cachedPipelines[func] = pipeline;

        return pipeline;
    }
    
    public Action<PacketContext> Build(Action<PacketContext> terminal) {
        var current = terminal;

        for (var i = middlewares.Count - 1; i >= 0; i--) {
            var next = current;
            var middleware = middlewares[i];
            current = ctx => middleware.Execute(ctx, next);
        }

        return current;
    }
}