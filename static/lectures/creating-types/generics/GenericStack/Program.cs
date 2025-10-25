namespace GenericStack;

class Program
{
    static void Main(string[] args)
    {
        Stack<int> stack = new Stack<int>();

        for (int i = 0; i < 10; i++)
        {
            stack.Push(i);
        }

        // string str = stack.Pop(); // Compilation error
        while (stack.Count > 0)
        {
            int number = stack.Pop();
            Console.WriteLine(number);
        }
    }
}