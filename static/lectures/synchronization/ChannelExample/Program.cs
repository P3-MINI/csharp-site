using System.Threading.Channels;

namespace ChannelExample;

class Program
{
    static async Task Main()
    {
        var channel = Channel.CreateBounded<int>(capacity: 5);

        Task producer = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Producing: {i}");
                await channel.Writer.WriteAsync(i); // Waits asynchronously if full
                await Task.Delay(500);
            }
            channel.Writer.Complete(); // Signal end of data
        });

        Task consumer = Task.Run(async () =>
        {
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"Consuming: {item}");
                await Task.Delay(1000);
            }
        });

        await Task.WhenAll(producer, consumer);
    }
}