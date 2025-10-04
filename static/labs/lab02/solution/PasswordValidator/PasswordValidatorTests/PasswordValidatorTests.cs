using PasswordValidatorLib;

namespace PasswordValidatorTests;

[TestClass]
public sealed class PasswordValidatorTests
{
    [TestMethod]
    public void Validate_ValidPassword_ReturnsEmptyErrorList()
    {
        // Arrange
        PasswordValidator validator = new PasswordValidator();
        string password = "Pass123!";
        
        // Act 
        var errorList = validator.Validate(password);
        
        // Assert
        Assert.AreEqual(errorList.Count, 0);
    }
    
    [TestMethod]
    public void Validate_PasswordHasNoSpecialCharacter_ReturnsNoSpecialCharacterError()
    {
        // Arrange
        PasswordValidator validator = new PasswordValidator();
        string password = "Password123";
        
        // Act 
        var errorList = validator.Validate(password);
        
        // Assert
        CollectionAssert.Contains(errorList, ValidationError.NoSpecialCharacter);
    }
    
    [TestMethod]
    public void ValidateLength_EmptyString_ReturnsFalse()
    {
        // Arrange
        PasswordValidator validator = new PasswordValidator();
        string password = "";
        
        // Act 
        var valid = validator.ValidateLength(password);
        
        // Assert
        Assert.IsFalse(valid);
    }
    
    [TestMethod]
    public void ValidateContainsDigit_PasswordWithDigit_ReturnsTrue()
    {
        // Arrange
        PasswordValidator validator = new PasswordValidator();
        string password = "777777";
        
        // Act 
        var valid = validator.ValidateContainsDigit(password);
        
        // Assert
        Assert.IsTrue(valid);
    }
    
    [TestMethod]
    public void ValidateContainsDigit_PasswordWithNoDigit_ReturnsFalse()
    {
        // Arrange
        PasswordValidator validator = new PasswordValidator();
        string password = "password";
        
        // Act 
        var valid = validator.ValidateContainsDigit(password);
        
        // Assert
        Assert.IsFalse(valid);
    }
}
