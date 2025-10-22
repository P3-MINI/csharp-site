using System.Reflection;
using MiniTest.Attributes;

namespace MiniTestRunner;

/// <summary>
/// Entry point for the MiniTestRunner application.
/// Responsible for loading test assemblies, discovering test classes and methods, and executing them.
/// </summary>
class Program
{
    /// <summary>
    /// Main method that runs the test runner.
    /// Iterates over provided assembly paths, loads each assembly, discovers tests, and executes them.
    /// </summary>
    /// <param name="args">An array of file paths to test assemblies.</param>
    static void Main(string[] args)
    {
        foreach (var path in args)
        {
            TestLoadContext? context = null;
            try
            {
                (context, var assembly) = LoadTestAssembly(path);
                var testClasses = FindAllTests(assembly);
                var results = new TestResults();
                foreach (var testClass in testClasses)
                {
                    results += testClass.Run();
                    Console.WriteLine(new string('#', Math.Min(80, Console.WindowWidth)));
                }

                Console.WriteLine($"Summary of running tests from {assembly.GetName().Name}:");
                results.Summarize();
            }
            finally
            {
                context?.Unload();
            }
        }
    }

    /// <summary>
    /// Loads a test assembly from the specified path using a custom <see cref="TestLoadContext"/>.
    /// </summary>
    /// <param name="path">The file path to the test assembly.</param>
    /// <returns>A tuple containing the <see cref="TestLoadContext"/> and the loaded <see cref="Assembly"/>.</returns>
    public static (TestLoadContext, Assembly) LoadTestAssembly(string path)
    {
        var context = new TestLoadContext(path);
        var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(path));
        return (context, assembly);
    }

    /// <summary>
    /// Discovers all test classes and their associated test methods in the given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for test classes.</param>
    /// <returns>A list of <see cref="TestClass"/> instances representing discovered tests.</returns>
    public static List<TestClass> FindAllTests(Assembly assembly)
    {
        using var _ = new ConsoleColoring(ConsoleColor.Yellow);
        var testClasses = new List<TestClass>();
        foreach (var type in assembly.ExportedTypes)
        {
            var testClass = type.GetCustomAttribute<TestClassAttribute>();
            if (type.GetConstructor(Type.EmptyTypes) is null)
            {
                Console.WriteLine($"Warning: Skipping test class {type.FullName}, because it has no parameterless constructor.");
                continue;
            }
            if (testClass is not null)
            {
                var testClassDescription = type.GetCustomAttribute<DescriptionAttribute>();
                var tests = new List<TestMethod>();
                TestSetup? beforeEach = null;
                TestSetup? afterEach = null;
                foreach (var method in type.GetMethods())
                {
                    var beforeEachAttribute = method.GetCustomAttribute<BeforeEachAttribute>();
                    var afterEachAttribute = method.GetCustomAttribute<AfterEachAttribute>();
                    var testMethod = method.GetCustomAttribute<TestMethodAttribute>();
                    if (testMethod is not null)
                    {
                        var description = method.GetCustomAttribute<DescriptionAttribute>();
                        var priority = method.GetCustomAttribute<PriorityAttribute>();
                        var parameters = method
                            .GetCustomAttributes<DataRowAttribute>()
                            .Where(attribute =>
                            {
                                var compatible = ValidateTypes(attribute.Data, method
                                    .GetParameters()
                                    .Select(param => param.GetModifiedParameterType())
                                    .ToArray());
                                if (!compatible)
                                {
                                    Console.WriteLine($"Skipping test {method.Name}({attribute.Description}), because it has incompatible parameters.");
                                }
                                return compatible;
                            })
                            .Select(attribute => new TestParameters(attribute.Description, attribute.Data))
                            .ToList();
                        tests.Add(new TestMethod(method, parameters, description?.Description, priority?.Priority ?? 0));
                    }

                    if (beforeEachAttribute is not null)
                    {
                        if (method.GetParameters().Length != 0)
                        {
                            Console.WriteLine($"Warning: Setup method {method.Name} takes {method.GetParameters().Length} parameters.");
                            continue;
                        }
                        beforeEach = new TestSetup(method);
                    }
                    if (afterEachAttribute is not null)
                    {
                        if (method.GetParameters().Length != 0)
                        {
                            Console.WriteLine($"Warning: Setup method {method.Name} takes {method.GetParameters().Length} parameters.");
                            continue;
                        }
                        afterEach = new TestSetup(method);
                    }
                }
                testClasses.Add(new TestClass(type, tests, beforeEach, afterEach, testClassDescription?.Description));
            }
        }

        return testClasses;
    }

    /// <summary>
    /// Validates that the provided objects match the expected parameter types.
    /// </summary>
    /// <param name="objects">The array of objects to validate.</param>
    /// <param name="types">The array of expected parameter types.</param>
    /// <returns><c>true</c> if all objects match the expected types; otherwise, <c>false</c>.</returns>
    private static bool ValidateTypes(object?[] objects, Type[] types)
    {
        if (objects.Length != types.Length)
        {
            return false;
        }

        for (var index = 0; index < objects.Length; index++)
        {
            var obj = objects[index];
            var type = types[index];

            if (!type.IsInstanceOfType(obj))
            {
                return false;
            }
        }

        return true;
    }
}