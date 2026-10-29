using ImageFilters;

// Usage: dotnet run [-- image.ppm]
Image input = args.Length > 0 ? Image.Load(args[0]) : Image.CreateTestPattern(640, 360);
Console.WriteLine($"Input: {(args.Length > 0 ? args[0] : "test pattern")} ({input.Width}x{input.Height})");
input.Save("input.ppm");
