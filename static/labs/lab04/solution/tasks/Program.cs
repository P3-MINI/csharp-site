namespace tasks;

public sealed class Program
{
	static void Main(string[] args)
	{
		var tasks = new IExecutable[]
		{
			new Task01(),
			new Task02(),
			new Task03(),
			new Task04()
		};

		Array.ForEach(
			tasks,
			task => task.Execute(args)
		);
	}
}