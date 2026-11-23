using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// Executes requests against the store. Knows nothing about the network.
/// </summary>
public sealed class RequestHandler(KeyValueStore store)
{
    public Response Handle(Request request)
    {
        switch (request)
        {
            case GetRequest(var key):
                return store.TryGet(key, out string? found) ? Response.Ok(found) : Response.NotFound();

            case SetRequest(var key, var value):
                store.Set(key, value);
                return Response.Ok();

            case DeleteRequest(var key):
                return store.Delete(key) ? Response.Ok() : Response.NotFound();

            case KeysRequest:
                return Response.WithKeys(store.Keys());

            default:
                return Response.Fail($"Unsupported request: {request.GetType().Name}");
        }
    }
}
