namespace Reflection;

public class Person
{
    private string _id;
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public DateTime Birthday { get; set; }
    
    public Person(string firstName, string lastName, DateTime? birthday = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Birthday = birthday ?? DateTime.Now;
        _id = GetId(Birthday);
    }
    
    public bool IsAdult()
    {
        return Birthday.AddYears(18) < DateTime.Today;
    }

    public static string GetId(DateTime birthday)
    {
        return $"{birthday.Year:0000}{birthday.Month:00}{birthday.Day:00}{Random.Shared.Next()%100000:00000}";
    }
}