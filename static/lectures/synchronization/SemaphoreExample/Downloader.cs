namespace SemaphoreExample;

public class Downloader(int concurrency)
{
    private readonly SemaphoreSlim _semaphore = new(concurrency);

    public async Task DownloadAsync(string url, string fileName, CancellationToken token = default)
    {
        Console.WriteLine($"[{fileName}] Waiting to start download...");

        await _semaphore.WaitAsync(token);
        
        Console.WriteLine($"[{fileName}] Downloading...");

        try
        {
            using HttpClient client = new HttpClient();

            byte[] data = await client.GetByteArrayAsync(url, token);

            Console.WriteLine($"[{fileName}] Downloaded {data.Length / 1024.0:0.00} KB");

            await File.WriteAllBytesAsync(fileName, data, token);
            Console.WriteLine($"[{fileName}] Download finished.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{fileName}] Error: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StartDownloadsAsync((string url, string filename)[] downloads, CancellationToken token = default)
    {
        var tasks = new Task[downloads.Length];
        for (int i = 0; i < downloads.Length; i++)
        {
            tasks[i] = DownloadAsync(downloads[i].url, downloads[i].filename, token);
        }

        await Task.WhenAll(tasks);
    }
}