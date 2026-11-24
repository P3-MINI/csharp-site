using System;
using System.Numerics;

namespace Task1;

public class Program {
    public static void Main() {
        // =============================================================
        // UNCOMMENT TO CHECK YOUR SOLUTION
        // =============================================================

        //var boxes = new Shelf<Box<int>>(3);
        //Console.WriteLine($"capacity:          {boxes.Space}");                             // 3
        //Console.WriteLine($"add Box(1):        {boxes.Add(new Box<int>(1))}");              // True
        //Console.WriteLine($"add Box(2):        {boxes.Add(new Box<int>(2))}");              // True
        //Console.WriteLine($"add Box(3):        {boxes.Add(new Box<int>(3))}");              // True
        //Console.WriteLine($"add Box(4) (full): {boxes.Add(new Box<int>(4))}");              // False

        //Console.WriteLine($"take:              {boxes.Take()}");                            // Box: 3
        //Console.WriteLine($"take:              {boxes.Take()}");                            // Box: 3
        //Console.WriteLine($"take:              {boxes.Take()}");                            // Box: 3
        //Console.WriteLine($"take from empty:   {boxes.Take()?.ToString() ?? "<null>"}");    // <null>
        //Console.WriteLine();

        // =============================================================

        //var intSum = new NumBox<int>(3) + new NumBox<int>(4);
        //Console.WriteLine($"NumBox + NumBox:                {intSum}");         // Box: 7

        //var doubleSum = new NumBox<double>(3) + new NumBox<double>(4);
        //Console.WriteLine($"NumBox + NumBox:                {doubleSum}");      // Box: 7

        //var strSum = new StringBox("x") + new StringBox("y");
        //Console.WriteLine($"StringBox + StringBox:          {strSum}");         // Box: x;y

        //Box<int> baseBox = intSum;
        //Console.WriteLine($"Upcast Box<int> also contains:  {baseBox}");        // Box: 7
        //Console.WriteLine();

        // =============================================================

        //var intShelf = new Shelf<NumBox<int>>(2);
        //intShelf.Add(new NumBox<int>(10));
        //intShelf.Add(new NumBox<int>(20));

        //// NumBox<int> derives from Box<int>, so Shelf<NumBox<int>> should be assignable to ITaker<Box<int>>
        //ITaker<Box<int>> covariantTaker = intShelf;                       
        //Console.WriteLine($"take: {covariantTaker.Take()}");                        // Box: 20
        //Console.WriteLine($"take: {covariantTaker.Take()}");                        // Box: 10
        //Console.WriteLine();
       
        //var doubleShelf = new Shelf<Box<double>>(2);

        //// Box<double> is the base of NumBox<double>, so Shelf<Box<double>> should be assignable to IAdder<NumBox<double>>.
        //IAdder<NumBox<double>> contravariantAdder = doubleShelf;         
        //contravariantAdder.Add(new NumBox<double>(40));         
        //contravariantAdder.Add(new NumBox<double>(30));        

        //Console.WriteLine($"take: {doubleShelf.Take()?.ToString() ?? "<null>"}");   // Box: 30
        //Console.WriteLine($"take: {doubleShelf.Take()?.ToString() ?? "<null>"}");   // Box: 40
        //Console.WriteLine();

        // =============================================================
    }
}
