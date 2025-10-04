using PasswordValidatorLib;
using Pastel;

var validator = new PasswordValidator();

Console.WriteLine("Enter password to check its strength. Type 'exit' to exit");

while (true)
{
    Console.Write("\n> ");
    string? password = Console.ReadLine();

    if (password == "exit")
    {
        break;
    }

    if (string.IsNullOrEmpty(password))
    {
        continue;
    }

    List<ValidationError> errors = validator.Validate(password);

    if (errors.Count == 0)
    {
        Console.WriteLine("✓ Password is valid and safe!".Pastel(ConsoleColor.Green));
    }
    else
    {
        Console.WriteLine("✗ Password is invalid:".Pastel(ConsoleColor.Red));
        foreach (var error in errors)
        {
            Console.WriteLine($"  - {GetErrorMessage(error)}".Pastel(ConsoleColor.Red));
        }
    }
}

string GetErrorMessage(ValidationError error) => error switch
{
    ValidationError.PasswordTooShort => "Password should contain at least 8 characters",
    ValidationError.NoUppercaseLetter => "Password should contain at least one uppercase letter",
    ValidationError.NoLowercaseLetter => "Password should contain at least one lowercase letter",
    ValidationError.NoDigit => "Password should contain at least one digit",
    ValidationError.NoSpecialCharacter => "Password should contain at least one special character",
    _ => "Unknown Error"
};
