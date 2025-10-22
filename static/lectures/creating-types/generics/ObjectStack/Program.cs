namespace ObjectStack;

class Program
{
    static void Main(string[] args)
    {
        Stack stack = new Stack();

        for (int i = 0; i < 10; i++)
        {
            stack.Push(i);
        }

        string str = (string)stack.Pop();  // Runtime error: InvalidCastException
        while (stack.Count > 0)
        {
            int number = (int)stack.Pop();
            Console.WriteLine(number);
        }
    }
}