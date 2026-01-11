namespace MemoryBarriers;

class Program
{
    private static void Main()
    {
        bool complete = false; 
        var thread = new Thread (() =>
        {
            bool toggle = false;
            while (!complete) toggle = !toggle;
        });
        thread.Start();
        Thread.Sleep(1000);
        complete = true;
        thread.Join();
    }
}