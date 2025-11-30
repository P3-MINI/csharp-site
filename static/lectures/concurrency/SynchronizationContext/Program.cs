namespace SynchronizationContexts;

class Program
{
    private static MySynchronizationContext? _context;

    static void Main(string[] args)
    {
        _context = new MySynchronizationContext();
        Thread worker = new Thread(() =>
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} thread started");
            _context?.Post(_ =>
            {
                Console.WriteLine($"Worker message from: {Thread.CurrentThread.Name}");
            }, null);
        }){Name = "Worker"};
        worker.Start();

        _context?.Post(_ =>
        {
            Console.WriteLine($"Main thread message from: {Thread.CurrentThread.Name}");
        }, null);
        worker.Join();
        _context?.Dispose();        
    }
}