using System.Net.Sockets;
using MiniKV.Client;
using MiniKV.Protocol;

// Stage 2: a one-shot client for testing the server, e.g.: dotnet run -- set user:1 Ala

Request request;
try
{
    request = CommandParser.Parse(string.Join(' ', args));
}
catch (FormatException e)
{
    Console.WriteLine(e.Message);
    return 1;
}

using var client = new TcpClient();
await client.ConnectAsync("localhost", KvProtocol.DefaultPort);
var messages = new MessageStream(client.GetStream());

await messages.WriteAsync(request);
Message? reply = await messages.ReadAsync();

Console.WriteLine(reply is Response response ? ResponseFormatter.Format(response) : $"Unexpected reply: {reply}");
return 0;
