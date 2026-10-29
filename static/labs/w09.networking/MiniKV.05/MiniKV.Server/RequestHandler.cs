using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// Executes requests against the store. Knows nothing about the network.
/// </summary>
public sealed class RequestHandler(KeyValueStore store)
{
    private readonly WatchRegistry _watchers = new();

    public Response Handle(Request request, ClientSession session)
    {
        switch (request)
        {
            case GetRequest(var key):
                return store.TryGet(key, out string? found) ? Response.Ok(found) : Response.NotFound();

            case SetRequest(var key, var value):
                store.Set(key, value);
                _watchers.Notify(key, value);
                return Response.Ok();

            case DeleteRequest(var key):
                if (!store.Delete(key))
                    return Response.NotFound();
                _watchers.Notify(key, null);
                return Response.Ok();

            case KeysRequest:
                return Response.WithKeys(store.Keys());

            case WatchRequest(var key):
                _watchers.Watch(key, session);
                return Response.Ok();

            case UnwatchRequest(var key):
                return _watchers.Unwatch(key, session) ? Response.Ok() : Response.NotFound();

            default:
                return Response.Fail($"Unsupported request: {request.GetType().Name}");
        }
    }

    public void ClientDisconnected(ClientSession session) => _watchers.RemoveSession(session);
}
