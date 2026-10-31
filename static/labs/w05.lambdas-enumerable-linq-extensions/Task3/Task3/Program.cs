namespace Task3;

public class Program {
    public static void Main(string[] args) {
        Database database = Database.GetInstance();

        // 1. List people (in alphabetical order by surname, then first name) who do not have a loyalty card.
        // Hints: join...into clause, Count() method
        Console.WriteLine("****** Question 1 ******");
        Console.WriteLine("Provided:");

        /* your code here */

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Bąk Magdalena");
        Console.WriteLine("Bąk Marek");
        Console.WriteLine("Wiśniewski Artur");

        // 2. Based on the PESEL number, select people who are more than 29 years old, by birth year. Display these people from oldest to youngest.
        // Requirement: do not count the same person multiple times (you may use the let clause)
        // Hint: A PESEL number from 2000 onwards differs in that 20 is added to the month value
        Console.WriteLine();
        Console.WriteLine("****** Question 2 ******");
        Console.WriteLine("Provided:");

        /* your code here */

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Kowalski Jan 117");
        Console.WriteLine("Marciniak Jerzy 35");
        Console.WriteLine("Antoniewski Michał 32");

        // 3. Determine the 3 people who have been using the store's services the longest.
        Console.WriteLine();
        Console.WriteLine("****** Question 3 ******");
        Console.WriteLine("Provided:");

        /* your code here */

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Nowak Andrzej 02.01.2017");
        Console.WriteLine("Antoniewski Michał 22.03.2017");
        Console.WriteLine("Kowalska Aneta 12.03.2018");

        // 4. Determine the 5 items with the highest total number of ordered units.
        Console.WriteLine();
        Console.WriteLine("****** Question 4 ******");
        Console.WriteLine("Provided:");

        /* your code here */

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Gaming chair 21");
        Console.WriteLine("Marker 11");
        Console.WriteLine("CD 10");
        Console.WriteLine("Pen 10");
        Console.WriteLine("Monitor 5");

        // 5. For each person, determine the total amount they spent in the store (taking into account the discount available to people with a loyalty card).
        // List the customers in alphabetical order by surname, then first name.
        Console.WriteLine();
        Console.WriteLine("****** Question 5 ******");
        Console.WriteLine("Provided:");

        /* your code here */

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Antoniewski Michał 1053");
        Console.WriteLine("Bąk Magdalena 2912");
        Console.WriteLine("Bąk Marek 3000");
        Console.WriteLine("Czerwińska Justyna 2593,5");
        Console.WriteLine("Kowalska Aneta 212,5");
        Console.WriteLine("Kowalski Jan 3654");
        Console.WriteLine("Marciniak Jerzy 13600");
        Console.WriteLine("Nowak Alina 1400");
        Console.WriteLine("Nowak Andrzej 2550");
        Console.WriteLine("Wiśniewski Artur 2025");

        Console.WriteLine();
    }
}
