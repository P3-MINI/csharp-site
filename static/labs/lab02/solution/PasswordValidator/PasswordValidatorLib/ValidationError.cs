namespace PasswordValidatorLib;

public enum ValidationError
{
    PasswordTooShort,
    NoLowercaseLetter,
    NoUppercaseLetter,
    NoDigit,
    NoSpecialCharacter
}