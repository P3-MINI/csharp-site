namespace Raytracing;

// Draws a text progress bar, e.g. [##########----------]  50%
public class ProgressBar(int total, int width = 40)
{
    private readonly Lock _lock = new();
    private int _lastPercent = -1;

    // Called concurrently from many threads
    public void Update(int done)
    {
        int percent = done * 100 / total;

        lock (_lock)
        {
            // Another thread may have already drawn a newer state
            if (percent <= _lastPercent)
            {
                return;
            }
            _lastPercent = percent;

            int filled = percent * width / 100;
            Console.Write($"\r[{new string('#', filled)}{new string('-', width - filled)}] {percent,3}%");
            if (percent == 100)
            {
                Console.WriteLine();
            }
        }
    }
}
