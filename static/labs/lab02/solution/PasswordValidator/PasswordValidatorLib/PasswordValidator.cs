namespace PasswordValidatorLib;

public class PasswordValidator
{
    public List<ValidationError> Validate(string password)
    {
        List<ValidationError> errors = new List<ValidationError>();

        if (!ValidateLength(password)) errors.Add(ValidationError.PasswordTooShort);
        if (!ValidateContainsSpecialCharacter(password)) errors.Add(ValidationError.NoSpecialCharacter);
        if (!ValidateContainsLowercase(password)) errors.Add(ValidationError.NoLowercaseLetter);
        if (!ValidateContainsUppercase(password)) errors.Add(ValidationError.NoUppercaseLetter);
        if (!ValidateContainsDigit(password)) errors.Add(ValidationError.NoDigit);

        return errors;
    }

    public bool ValidateLength(string password)
    {
        return password.Length >= 8;
    }

    public bool ValidateContainsSpecialCharacter(string password)
    {
        const string specialCharacters = @"!@#$%^&*(),.?'"";:{}|<>[]";
        foreach (var c in password)
        {
            if (specialCharacters.Contains(c)) return true;
        }

        return false;
    }

    public bool ValidateContainsDigit(string password)
    {
        foreach (var c in password)
        {
            if (char.IsDigit(c)) return true;
        }

        return false;
    }

    public bool ValidateContainsLowercase(string password)
    {
        foreach (var c in password)
        {
            if (char.IsAsciiLetterLower(c)) return true;
        }

        return false;
    }

    public bool ValidateContainsUppercase(string password)
    {
        foreach (var c in password)
        {
            if (char.IsAsciiLetterUpper(c)) return true;
        }

        return false;
    }
}