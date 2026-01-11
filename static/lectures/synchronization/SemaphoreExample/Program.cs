namespace SemaphoreExample;

class Program
{
    private static readonly (string, string)[] Downloads = [
        ("https://pages.mini.pw.edu.pl/~hermant/Tomek.jpg", "hermant.jpg"),
        ("https://pages.mini.pw.edu.pl/~aszklarp/images/me.jpg", "aszklarp.jpg"),
        ("https://pages.mini.pw.edu.pl/~rafalkoj/templates/mini/images/photo.jpg", "rafalkoj.jpg"),
        ("https://pages.mini.pw.edu.pl/~kaczmarskik/krzysztof.jpg", "kaczmarskik.jpg"),
        ("https://cadcam.mini.pw.edu.pl/static/media/kadra8.7b107dbb.jpg", "sobotkap.jpg")
    ];
    
    private static async Task Main()
    {
        CancellationTokenSource cancellation = new CancellationTokenSource();
        _ = Task.Run(() =>
        {
            Console.WriteLine("Press any key to interrupt...");
            Console.ReadKey();
            cancellation.Cancel();
        });
        Downloader downloader = new Downloader(2);
        try
        {
            await downloader.StartDownloadsAsync(Downloads, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled");
        }
    }
}