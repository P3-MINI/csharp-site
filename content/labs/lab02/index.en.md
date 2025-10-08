---
title: "Lab02"
weight: 10
---

# Lab 2: MSBuild, Unit Tests, .NET CLI

## Task 1: MSBuild

Your task is to modify the [`CppProject.proj`](/labs/lab02/start/CppProject/CppProject.proj) file to add the following functionalities:

> If you are working on Windows, you start with [`CppProject.proj`](/labs/lab02/start/CppProjectWindows/CppProject.proj) instead. You will be working with `cl.exe` compiler. All tools are available in the Developer Command Prompt for Visual Studio. You can find instructions on how to open the developer console in the [Visual Studio documentation](https://learn.microsoft.com/visualstudio/ide/reference/command-prompt-powershell).

{{< tabs >}}
{{% tab "Linux" %}} 
  {{% hint info %}}
  **Starting code** 
  {{< filetree dir="labs/lab02/start/CppProject" >}}
  {{% /hint %}}
{{% /tab %}}
{{% tab "Windows" %}} 
  {{% hint info %}}
  **Starting code** 
  {{< filetree dir="labs/lab02/start/CppProjectWindows" >}}
  {{% /hint %}}
{{% /tab %}}
{{< /tabs >}}

### 1. `Debug` and `Release` Configuration

The goal is to add support for two build configurations to the project: `Debug` and `Release`.
- **Debug**: The development configuration, which should include debugging symbols and have optimizations disabled.
- **Release**: The production configuration, which should be optimized for performance and not include debugging symbols.

**Requirements:**
- Add a `Configuration` property that will be set to `Debug` by default.
- Use conditional property groups (`PropertyGroup`) to define different compiler flags (for debugging symbols and optimization) depending on the active configuration.
- Modify the targets to use the defined flags; `-O0` and `-g` (`/Od` `/Zi` for `cl.exe`) for the `Debug` configuration; `-O2` (`/O2` for `cl.exe`) for the `Release` configuration.
- Modify the `OutputPath` so that the output files for each configuration go into separate subdirectories (e.g., `build/Debug/` and `build/Release/`).

When you're done, build the application from the command line in both the development and production configurations.

### 2. Incremental Builds

To speed up the build process, incremental compilation is often implemented. This means that only files that have been changed since the last compilation should be recompiled. MSBuild accomplishes this by comparing the timestamps of the files defined in the `Inputs` and `Outputs` attributes of a given target. If all output files (`Outputs`) are newer than all input files (`Inputs`), MSBuild skips the execution of that target, saving time.

**Requirements:**
- Use the `Inputs` and `Outputs` attributes in the `Compile` and `Link` targets.
- The `Compile` target for a given `.cpp` file should only run if the `.cpp` file itself or any of the header files (`.h`) in the project is newer than its corresponding object file (`.o`).
- The `Link` target should only run if any of the object files are newer than the executable file.

Clean the project with the `Clean` target, then build it twice with the `Build` target.

### 3. Creating a Distribution Package

The goal is to automate the creation of a `.zip` package containing the finished application and additional files.

**Requirements:**
- Add a new target named `CreateDist` to the project.
- The `CreateDist` target should depend on the `Build` target.
- Add a `Version` property (e.g., `1.0.0`) to be used in the package name.
- Add an item group where you define a `DistFiles` item containing `README.md` and `LICENSE`.
- In the `CreateDist` target:
    - Copy the executable file and the distribution files to a temporary directory.
    - Use the [`ZipDirectory`](https://learn.microsoft.com/visualstudio/msbuild/zipdirectory-task) task to pack the contents of the temporary directory.
    - The name of the resulting archive file should have the format `$(OutputName)-$(Version).zip`.
    - Remove the temporary directory.

Call the `CreateDist` target for the `Release` configuration.

### Example Solution

{{< tabs >}}
{{% tab "Linux" %}} 
  {{% hint info %}}
  **Solution** 
  {{< filetree dir="labs/lab02/solution/CppProject" >}}
  {{% /hint %}}
{{% /tab %}}
{{% tab "Windows" %}} 
  {{% hint info %}}
  **Solution** 
  {{< filetree dir="labs/lab02/solution/CppProjectWindows" >}}
  {{% /hint %}}
{{% /tab %}}
{{< /tabs >}}

## Task 2: .NET SDK

In the second task, we will work with SDK-style projects. Solutions are used to group related projects. Solutions have nothing to do with MSBuild; they are Visual Studio files but are also supported by other IDEs. When you open a solution, all the projects that are part of it will open in the IDE. The `dotnet` tool also allows you to work with solution files from the command line.

It's worth getting familiar with the `dotnet` command. The `--help` option will list all available commands—familiarize yourself with them. If you want to learn more about a specific command, use the `--help` option with that command.

```shell
dotnet --help
dotnet [command] --help
```

### 1. Creating a Solution and Projects

> You can do this task in two ways: from the command line using the `dotnet` command, or using an IDE of your choice. On Windows, you can choose between `Visual Studio` and `Rider`; on Linux, `Rider` is available.

We'll start by creating a solution and two projects: a library and a console application. The console project will be a command-line interface for the library. We will be creating a password validation application. Come up with a name for the solution and projects. It could be `PasswordValidator` for the solution and `PasswordValidator.App`, `PasswordValidator.Lib` for the projects if you don't have better ideas.

```shell
# Create a solution
dotnet new sln --output <SolutionName>
cd <SolutionName>

# Create two projects inside <SolutionName>
dotnet new console --output <ConsoleProjectName>
dotnet new classlib --output <LibraryProjectName>

# Add projects to the solution:
dotnet sln add <ConsoleProjectName> <LibraryProjectName>

# Generate .gitignore file
dotnet new gitignore
```

After creation, run the console application: either through the IDE or with `dotnet run`.

For the code from the `PasswordValidatorLib` project to be visible in the console application, you need to add a reference to it in the `PasswordValidatorApp` project. This can be done in several ways:

1. `dotnet` CLI: `dotnet add PasswordValidatorApp reference PasswordValidatorLib`
2. Through the IDE:
   * [Visual Studio](https://learn.microsoft.com/visualstudio/ide/how-to-add-or-remove-references-by-using-the-reference-manager)
   * [Rider](https://www.jetbrains.com/help/rider/Extending_Your_Solution.html#project_assembly_references)
3. By manually editing the `PasswordValidatorApp.csproj` project file

Regardless of the chosen method, you should see the following entry in the console project's file:

```xml
  <ItemGroup>
    <ProjectReference Include="..\PasswordValidatorLib\PasswordValidatorLib.csproj" />
  </ItemGroup>
```

`ProjectReference` items are projects whose code we can use in this project. They will be built and included in this project.

### 2. Library Part

We can start by deleting the template file `Class1.cs`. We will create two files:

- `ValidationError.cs` with a public enum of possible errors.
- `PasswordValidator.cs` with a public class of the same name, and in it, a method `public List<ValidationError> Validate(string password)`.

To avoid putting all the long logic in one method, we will divide the detection of specific password features into separate methods:

- `public bool ValidatePasswordLength(string password)`: checks if the password is at least 8 characters long.
- `public bool ValidatePasswordHasLowerCaseLetter(string password)`: checks if the password contains a lowercase letter.
- `public bool ValidateContainsSpecialCharacter(string password)`: checks if it contains one special character from the set: `!@#$%^&*(),.?'";:{}|<>[]`.
- ... etc.

For each method, add a corresponding enum value to `ValidationError`.

### 3. Console Part

In `Program.cs`, create a new `PasswordValidator` object and, in a loop, ask the user for a password.

- If the password is correct, display the message `"✓ Password is valid and safe!"`
- If the password is incorrect, display the message `"✗ Password is invalid:"`
  - For each unmet rule, print its verbal description on a new line, e.g., `"Password should contain at least 8 characters"`.
  - Add a function `string GetErrorMessage(ValidationError error)` that will return a text description of the rule.
- If the user types `exit`, break the loop and end the program.

### 4. `NuGet`

`NuGet` is the official package manager for the .NET platform. Imagine you are building an application and need to implement some functionality, e.g., coloring text in the console, logging errors, or working with JSON files. Instead of writing all this code from scratch, you can use a ready-made library (or "package") that someone has already created, tested, and shared.

Available packages can be searched on [nuget.org](nuget.org), via the CLI `dotnet package search <search term>`, or through an IDE. You can find out how to do this for Visual Studio in the [NuGet documentation](https://learn.microsoft.com/nuget/quickstart/install-and-use-a-package-in-visual-studio#nuget-package-manager), and for Rider in [its documentation](https://www.jetbrains.com/help/rider/Using_NuGet.html).

We will add output coloring to our console application. We will use the ready-made `Pastel` library available in the `NuGet` repository. To add the package to the project, you can do it in two ways: via the `dotnet` command: `dotnet add PasswordValidatorApp package Pastel`, or through an IDE.

After adding, you should notice a new entry in the project file that declares the project's dependency on the NuGet package. Items in `PackageReference` are libraries that will be downloaded during the build and can be used in the project.

```xml
  <ItemGroup>
    <PackageReference Include="Pastel" Version="7.0.0" />
  </ItemGroup>
```

Next, add syntax coloring to the application.

1. Import the library: `using Pastel;`
2. Replace the printed strings `"<Text>"` with `"<Text>".Pastel(ConsoleColor.<Color>)`, and set:
   - green color if the password was correct
   - red color if it was incorrect

## Task 3: Unit Tests

The last type of project we will work with is unit tests. A unit test project is built like a regular library. This library is then **input** for a *test runner*, which searches for methods marked with the `[Test]` attribute (i.e., tests) in such a library and runs them. Conventionally, we assume that a test has passed if it did not throw an exception. Methods from the "Assert" family, which are used to check conditions, throw an exception if the condition is not met.

### 0. What are unit tests?

A unit test is a piece of code that automatically checks the correctness of a "unit" of application code – most often a single method or class.

The main goal is to make sure that a given piece of code works exactly as we expect, in isolation from the rest of the code. This allows for early detection of errors and protects against breaking existing functionalities.

A good unit test is written according to the simple **Arrange-Act-Assert (AAA)** pattern:
1. **Arrange:** You prepare the conditions and input data.
2. **Act:** You call the method being tested.
3. **Assert:** You check if the result is consistent with expectations.

In C#, we have 3 different libraries for unit tests:

- `MSTest`
- `xUnit`
- `NUnit`

Generally, we will use `MSTest`, but they all work on the same principle, do the same thing, and differ only slightly in terminology, e.g., `Fact` vs `Test`.

### 1. Creating a Test Project

To add a unit test project, we can again do it in two ways: through the CLI or through an IDE. Additionally, the test project should have a reference to the project being tested (in our case, `PasswordValidatorLib`).

```shell
dotnet new mstest --output PasswordValidatorTests
dotnet sln add PasswordValidatorTests
dotnet add PasswordValidatorTests reference PasswordValidatorLib
```

In the newly generated project, you should see a class in the `Test1.cs` file, which for now contains 1 test. A class that can contain tests is marked with the `[TestClass]` attribute, and unit tests (methods) are marked with the `[TestMethod]` attribute.

```csharp
namespace PasswordValidatorTests;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void TestMethod1()
    {
    }
}
```

To run the tests, you can run `dotnet test` in the console, or do it through an IDE. You can find instructions on how to do this in the [Visual Studio documentation](https://learn.microsoft.com/visualstudio/test/run-unit-tests-with-test-explorer) or the [Rider documentation](https://www.jetbrains.com/help/rider/Getting_Started_with_Unit_Testing.html#step-3-run-the-tests). Since the method is now empty, the test should pass.

### 2. Unit Tests

We'll start by renaming the test file and the test class to `PasswordValidatorTests`. In it, we will also rename the existing test to `ValidatePassword_ValidPassword_ReturnsEmptyErrorList`. This is a common convention for naming tests, in which we include what we are testing, what the input is, and what the expected behavior is. We will create this test according to the **Arrange-Act-Assert** pattern:

```csharp
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
```

In this way, for the `Validate` method, we have defined the behavior that this method should fulfill. The goal is to write tests for all key scenarios: correct operation, error handling, and edge cases (e.g., empty string, `null`, boundary values).

A good unit test should be:

* **fast** - there can be thousands of tests in a project; we want to get quick feedback on whether our changes cause regression.
* **independent** - a test should check only one, specific "unit" of code and be isolated from external dependencies (database, network, UI). Applying the [**SOLID**](https://en.wikipedia.org/wiki/SOLID) principles (especially the **Dependency Inversion Principle**) is key to achieving this isolation, as it allows the use of so-called "mocks" instead of real dependencies.
* **repeatable** - a test must give the same result every time, regardless of the environment in which it is run (e.g., on a developer's machine, on a CI/CD server). It should not depend on external factors such as the current date/time, random values, or system configuration.
* **simple** - a test should be short - about 3-5 lines - and self-documenting. It is important that in the test we only define the input, call the test method, and check the output; under no circumstances do we write logic in it, especially the logic of the tested method.

Sometimes, you start by writing unit tests, i.e., defining the behavior of functions, and only then write the implementation of the tested methods until all tests pass. This approach is called *Test Driven Development (TDD)*.

Now try to write some unit tests yourself. For example:

- `Validate_PasswordHasNoSpecialCharacter_ReturnsNoSpecialCharacterError`
- `ValidateLength_EmptyString_ReturnsFalse`
- `ValidateContainsDigit_PasswordWithDigit_ReturnsTrue`
- `ValidateContainsDigit_PasswordWithNoDigit_ReturnsFalse`

You may need other assertion methods: `CollectionAssert.Contains`, `Assert.IsFalse`, `Assert.IsTrue`.

### Example Solution

An example solution can be found in [PasswordValidator](/labs/lab02/solution/PasswordValidator).

> [!NOTE]
> **PasswordValidator**
> {{< filetree dir="labs/lab02/solution/PasswordValidator" >}}
