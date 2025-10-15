namespace TypeCrafter
{
    public sealed class Program
    {
        static void Main()
        {
            CraftInstance<Customer>();
            CraftInstance<Invoice>();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void CraftInstance<T>()
        {
            try
            {
                var craftedObject = TypeCrafter.CraftInstance<T>();
                Console.WriteLine(craftedObject);
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Wrong format for parsing: {ex}");
                Console.WriteLine("Attempting to craft object once again...");

                CraftInstance<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Received unexpected exception with message: {ex.Message} " +
                    $"Cannot create object of type {typeof(T)}.");
            }
        }
    }
}