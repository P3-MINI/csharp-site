namespace Task3;

public record class Customer(uint CustomerId, string FirstName, string LastName, string PESEL);

public record class LoyaltyCard(uint CustomerId, double Discount);

public record class Product(uint ProductId, string Name, double Price);

public record class Order(uint OrderId, uint CustomerId, DateTime Date);

public record class OrderProduct(uint OrderId, uint ProductId, uint Quantity);


public class Database {
    public List<Customer> Customers { get; private set; } = [];
    public List<LoyaltyCard> LoyaltyCards { get; private set; } = [];
    public List<Product> Products { get; private set; } = [];
    public List<Order> Orders { get; private set; } = [];
    public List<OrderProduct> OrderProducts { get; private set; } = [];


    public static Database GetInstance() {
        return new Database() {
            Customers =
            [
                new Customer(1, "Jan", "Kowalski", "03112323433"),
                new Customer(2, "Andrzej", "Nowak", "92060363493"),
                new Customer(3, "Artur", "Wiśniewski", "04211263493"),
                new Customer(4, "Alina", "Nowak", "96123012364"),
                new Customer(5, "Magdalena", "Bąk", "00322123443"),
                new Customer(6, "Justyna", "Czerwińska", "99010323443"),
                new Customer(7, "Marek", "Bąk", "02260323433"),
                new Customer(8, "Jerzy", "Marciniak", "85013023433"),
                new Customer(9, "Aneta", "Kowalska", "96100923443"),
                new Customer(10, "Michał", "Antoniewski", "88010123353")
            ],

            LoyaltyCards =
            [
                new LoyaltyCard(1, 0.1),
                new LoyaltyCard(2, 0.25),
                new LoyaltyCard(4, 0.2),
                new LoyaltyCard(6, 0.05),
                new LoyaltyCard(8, 0.15),
                new LoyaltyCard(9, 0.15),
                new LoyaltyCard(10, 0.1),
            ],

            Products =
            [
                new Product(1, "Phone", 1000),
                new Product(2, "TV", 2000),
                new Product(3, "Monitor", 1000),
                new Product(4, "Mouse", 200),
                new Product(5, "Hairdryer", 100),
                new Product(6, "Iron", 150),
                new Product(7, "Gaming chair", 800),
                new Product(8, "Keyboard", 300),
                new Product(9, "Pen", 2),
                new Product(10, "Phone Case", 50),
                new Product(11, "Laptop", 2000),
                new Product(12, "AA Batteries", 4),
                new Product(13, "Printer Ink", 40),
                new Product(14, "Marker", 5),
                new Product(15, "CD", 1),
                new Product(16, "HDMI Cabel", 30),
                new Product(17, "Washing Machine", 1000),
                new Product(18, "Fridge", 2000),
                new Product(19, "Scotch Tape", 5),
                new Product(20, "Scissors", 5),
                new Product(21, "Toaster", 100),
                new Product(22, "Electric Kettle", 100)
            ],

            Orders =
            [
                new Order(1, 1, new DateTime(2019, 12, 12)),
                new Order(2, 2, new DateTime(2017, 1, 2)),
                new Order(3, 2, new DateTime(2019, 12, 12)),
                new Order(4, 3, new DateTime(2019, 6, 30)),
                new Order(5, 3, new DateTime(2019, 7, 7)),
                new Order(6, 3, new DateTime(2019, 7, 14)),
                new Order(7, 4, new DateTime(2019, 12, 23)),
                new Order(8, 4, new DateTime(2018, 10, 21)),
                new Order(9, 5, new DateTime(2019, 10, 1)),
                new Order(10, 5, new DateTime(2019, 10, 2)),
                new Order(11, 6, new DateTime(2019, 11, 12)),
                new Order(12, 7, new DateTime(2019, 2, 28)),
                new Order(13, 8, new DateTime(2019, 5, 15)),
                new Order(14, 9, new DateTime(2018, 3, 12)),
                new Order(15, 10, new DateTime(2017, 3, 22)),
                new Order(16, 10, new DateTime(2017, 3, 23)),
                new Order(17, 10, new DateTime(2017, 3, 24)),
                new Order(18, 10, new DateTime(2017, 3, 25)),
                new Order(19, 2, new DateTime(2017, 8, 12)),
            ],

            OrderProducts =
            [
                new OrderProduct(1, 2, 2),
                new OrderProduct(1, 16, 2),
                new OrderProduct(2, 21, 1),
                new OrderProduct(2, 22, 1),
                new OrderProduct(3, 1, 3),
                new OrderProduct(3, 10, 3),
                new OrderProduct(4, 15, 10),
                new OrderProduct(4, 14, 1),
                new OrderProduct(5, 20, 1),
                new OrderProduct(5, 19, 1),
                new OrderProduct(6, 11, 1),
                new OrderProduct(7, 5, 1),
                new OrderProduct(7, 6, 1),
                new OrderProduct(7, 17, 1),
                new OrderProduct(8, 4, 1),
                new OrderProduct(8, 8, 1),
                new OrderProduct(9, 3, 2),
                new OrderProduct(9, 7, 1),
                new OrderProduct(10, 9, 10),
                new OrderProduct(10, 12, 3),
                new OrderProduct(10, 13, 2),
                new OrderProduct(11, 3, 2),
                new OrderProduct(11, 4, 2),
                new OrderProduct(11, 8, 1),
                new OrderProduct(11, 16, 1),
                new OrderProduct(12, 18, 1),
                new OrderProduct(12, 17, 1),
                new OrderProduct(13, 7, 20),
                new OrderProduct(14, 5, 1),
                new OrderProduct(14, 6, 1),
                new OrderProduct(15, 22, 1),
                new OrderProduct(16, 13, 1),
                new OrderProduct(17, 16, 1),
                new OrderProduct(18, 3, 1),
                new OrderProduct(19, 14, 10),
            ]
        };
    }
}
