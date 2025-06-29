namespace tasks;

public sealed class Task03 : IExecutable
{
	public void Execute(string[] args)
	{
		Console.WriteLine($"Executing {nameof(Task03)}...");

		// 10 elements of an arithmetic sequence starting at 3 with a difference of 8
		{
			var a = 3;
			var r = 8;

			var list1 = new List<int>(capacity: 10);

			Fill(list1, 10, 
				() => 
				{ 
					var b = a; 
					a += r; 
					return b; 
				});

			Console.WriteLine(string.Join(", ", list1));
		}

		// 10 elements of the Fibonacci sequence
		{
			var f0 = 0;
			var f1 = 1;
			var counter = 0;

			var list2 = new List<int>(capacity: 10);

			Fill(list2, 10,
				() =>
				{
					int f;
					if (counter == 0)
					{
						f = f0;
					}
					else if (counter == 1)
					{
						f = f1;
					}
					else
					{
						(f1, f0) = (f1 + f0, f1);
						f = f1;
					}
					counter++;
					return f;
				});

			Console.WriteLine(string.Join(", ", list2));
		}

		// 10 random integers in the range [5, 50]
		var random = new Random();
		
		{

			var list3 = new List<int>(capacity: 10);

			Fill(list3, 10, () => random.Next(5, 51));

			Console.WriteLine(string.Join(", ", list3));
		}

		// 10 elements of 0 or 1 with a given probability (e.g., P(1) = 0.3).
		{
			var list4 = new List<int>(capacity: 10);

			Fill(list4, 10, () => random.NextDouble() <= 0.3 ? 1 : 0);

			Console.WriteLine(string.Join(", ", list4));
		}

		// 10 random elements from the first 10 prime numbers [2, 3, 5, 7, 11, 13, 17, 19, 23, 29]
		{
			var numbers = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };

			var list5 = new List<int>(capacity: 10);

			Fill(list5, 10, () => numbers[random.Next(maxValue: numbers.Length)]);

			Console.WriteLine(string.Join(", ", list5));
		}

		// A Markov chain of length 20 starting in state `1`, defined by the transition matrix:
		// |     |  1  |  2  |  3  |
		// | --- | --- | --- | --- |
		// |  1  | 0.1 | 0.6 | 0.3 |
		// |  2  | 0.4 | 0.2 | 0.4 |
		// |  3  | 0.5 | 0.3 | 0.2 |
		{
			var transitions = new Dictionary<int, List<(int, double)>>
			{
				[1] = [(1, 0.1), (2, 0.6), (3, 0.3)],
				[2] = [(1, 0.4), (2, 0.2), (3, 0.4)],
				[3] = [(1, 0.5), (2, 0.3), (3, 0.2)],
			};

			var state = 1;

			var list6 = new List<int>(capacity: 20);

			Fill(list6, 20, () =>
			{
				var roll = random.NextDouble();
				var cumulative = 0.0;
			
				foreach (var (nextState, probability) in transitions[state])
				{
					cumulative += probability;
					if (roll <= cumulative)
					{
						state = nextState;
						break;
					}
				}

				return state;
			});

			Console.WriteLine(string.Join(", ", list6));
		}
	}

	public static void Fill(List<int> collection, int length, Func<int> generator)
	{
		while(length-- > 0)
		{
			collection.Add(generator());
		}
	}
}