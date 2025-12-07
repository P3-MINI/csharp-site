using System.IO.Pipes;
using System.Text;

class KeyValueClient
{
    const int ConnectionTimeoutMs = 3000;

    public static async Task Main()
    {
        using var client = new NamedPipeClientStream(".", "kv_pipe", PipeDirection.InOut);

        Console.WriteLine("Connecting to server...");

        try
        {
            await client.ConnectAsync(ConnectionTimeoutMs);
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Cannot connect to server - exiting the program");
            return;
        }
        
        Console.WriteLine("Connected!");

        var reader = new StreamReader(client, Encoding.UTF8);
        var writer = new StreamWriter(client, Encoding.UTF8);

        Console.WriteLine("Enter commands (SET, GET, DELETE, EXIT):");

        while (true)
        {
            Console.Write("> ");
            string? cmd = Console.ReadLine();
            if (cmd == null)
                break;
                
            if (cmd.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            string? response = await GetResponse(writer, reader, cmd);
            if (response == null)
                break;
            
            Console.WriteLine(response);
        }

        Console.WriteLine("Disconnected.");
    }


    static async Task<string?> GetResponse(StreamWriter writer, StreamReader reader, string cmd)
    {
        try
        {
            await writer.WriteLineAsync(cmd);
            await writer.FlushAsync();
            return await reader.ReadLineAsync();
        }
        catch (IOException)
        {
            Console.WriteLine("Problem with server communication");
            return null;
        }
    }
}