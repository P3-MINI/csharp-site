using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Reflection;

class Program
{
    static void Main(string[] args)
    {
        GettingType();
        QueryingMembers();
        QueryingWithTypeInfo();
        CallingMethods();
        CallingConstructors();
        CallingProperties();
        SettingFields();
        MethodInfoToDelegate();
        ReflectingAssemblies();
    }

    public static void GettingType()
    {
        PrintCurrentMethodName();
        Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
        Type personType1 = typeof(Person);
        Type personType2 = person.GetType();
        Type? personType3 = Type.GetType("Reflection.Person");
        Console.WriteLine($"Type 1: {personType1.FullName}");
        Console.WriteLine($"Type 2: {personType2.FullName}");
        Console.WriteLine($"Type 3: {personType3?.FullName}");
    }

    public static void QueryingMembers()
    {
        PrintCurrentMethodName();
        MemberInfo[] members = typeof(Person).GetMembers();
        foreach (MemberInfo member in members)
        {
            Console.WriteLine($"{member.MemberType,12}: {member}");
        }
    }

    public static void QueryingWithTypeInfo()
    {
        PrintCurrentMethodName();
        IEnumerable<MemberInfo> members = typeof(Person).GetTypeInfo().DeclaredMembers;
        foreach (MemberInfo member in members)
        {
            Console.WriteLine($"{member.MemberType,12}: {member}");
        }
    }

    public static void CallingMethods()
    {
        PrintCurrentMethodName();
        Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
        MethodInfo? method = typeof(Person).GetMethod("IsAdult");
        if (method is null)
        {
            Console.WriteLine("Method `IsAdult` not found");
        }
        else
        {
            if (method.GetParameters().Length == 0)
            {
                bool? isAdult = method.Invoke(person, [/*parameters*/]) as bool?;
                Console.WriteLine($"Is Adult: {isAdult}");
            }
            else
            {
                Console.WriteLine("Method `IsAdult` is not parameterless");
            }
        }
    }

    public static void CallingConstructors()
    {
        PrintCurrentMethodName();
        ConstructorInfo? constructor = typeof(Person)
            .GetConstructor([typeof(string), typeof(string), typeof(DateTime?)]);
        Person? person = constructor?.Invoke(["John", "Doe", DateTime.Now.AddYears(-42)]) as Person;
        Console.WriteLine($"Name: {person?.FullName}, Birthday: {person?.Birthday:d}");
    }
    
    public static void CallingProperties()
    {
        PrintCurrentMethodName();
        Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
        PropertyInfo? property = typeof(Person).GetProperty("Birthday");
        if (property is null)
        {
            Console.WriteLine("Property `Birthday` not found");
        }
        else
        {
            DateTime? birthday = property.GetValue(person) as DateTime?;
            property.SetValue(person, birthday?.AddYears(-2));
            Console.WriteLine($"Age: {DateTime.Now.Year - birthday?.Year}");
        }
    }
    
    public static void SettingFields()
    {
        PrintCurrentMethodName();
        Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
        FieldInfo? field = typeof(Person).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field is null)
        {
            Console.WriteLine("Field `_id` not found");
        }
        else
        {
            string? id = field.GetValue(person) as string;
            Console.WriteLine($"Id before: {id}");
            field.SetValue(person, id?.Substring(0, 8));
            Console.WriteLine($"Id after: {field.GetValue(person)}");
        }
    }
    
    public static void MethodInfoToDelegate()
    {
        PrintCurrentMethodName();
        Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
        MethodInfo? method = typeof(Person).GetMethod(nameof(Person.IsAdult));
        if (method is not null)
        {
            int times = 1_000_000;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                method.Invoke(person, null);
            }
            sw.Stop();
            Console.WriteLine($"Invoke x{times}: {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            // Binding to a delegate:
            Func<bool> isAdult = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), person, method);
            for (int i = 0; i < times; i++)
            {
                isAdult();
            }
            sw.Stop();
            Console.WriteLine($"Delegate x{times}: {sw.ElapsedMilliseconds}ms");
        }
    }
    
    public static void ReflectingAssemblies()
    {
        PrintCurrentMethodName();
        Assembly? assembly;
        assembly = Assembly.GetEntryAssembly();
        assembly = Assembly.GetCallingAssembly();
        assembly = Assembly.GetExecutingAssembly();
        assembly = Assembly.GetAssembly(typeof(Person));
        if (assembly is null) return; 
        foreach (var type in assembly.GetTypes())
        {
            Console.WriteLine(type.FullName);
        }
    }
    
    private static void PrintCurrentMethodName([CallerMemberName] string caller = "")
    {
        Console.WriteLine("***************************************");
        Console.WriteLine($"* Method: {caller,27} *");
        Console.WriteLine("***************************************");
    }
}
