using BuilderGenerator;

namespace App
{
    [GenerateBuilder]
    public partial class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var user = User.CreateBuilder()
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithAge(30)
                .Build();

            System.Console.WriteLine($"Created user: {user.FirstName} {user.LastName}, Age: {user.Age}");
        }
    }
}
