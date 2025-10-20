namespace EnumerableStack;

class Program
{
    static void Main(string[] args)
    {
        Stack<string> stack = new Stack<string>();
        stack.Push("The");
        stack.Push("quick");
        stack.Push("brown");
        stack.Push("fox");
        stack.Push("jumps");
        stack.Push("over");
        stack.Push("the");
        stack.Push("lazy");
        stack.Push("dog");

        foreach (var str in stack)
        {
            Console.WriteLine(str);
        }
    }
}
