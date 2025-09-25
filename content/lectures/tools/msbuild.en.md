---
title: "MSBuild"
---

# Microsoft Build Engine

## Introduction

MSBuild is a tool for building applications. It is part of the .NET platform and is used to automate the processes of compiling code, testing, packaging, and publishing applications. It is the engine that Visual Studio uses to build projects, but it can also be run independently from the command line, which is particularly useful for automated build processes on servers (CI/CD).

## MSBuild Project Files

MSBuild uses XML-based project files. In these files, a developer can define how the build process should proceed. These files typically have extensions like `.csproj`, `.vbproj`, or generally `.proj`.

### Example Project File

Let's consider a simple C++ console application, `main.cpp`:

```cpp
#include <print>

int main() 
{
    std::println("Hello, world!");
    return 0;
}
```

Below is an MSBuild project file, `HelloMSBuild.proj`, that compiles this application:

```xml
<Project DefaultTargets="Build">
  <PropertyGroup>
    <Compiler Condition="'$(Compiler)' == ''">clang++</Compiler>
    <CppVersion Condition="'$(CppVersion)' == ''">c++23</CppVersion>
    <OutputPath>$(SolutionDir)bin/</OutputPath>
    <OutputName>program</OutputName>
  </PropertyGroup>
  
  <ItemGroup>
    <CppSource Include="**/*.cpp" />
  </ItemGroup>
  
  <Target Name="Build" DependsOnTargets="Link">
    <Message Text="Building with $(Compiler)..." Importance="high" />
  </Target>
  
  <Target Name="Compile">
    <MakeDir Directories="$(OutputPath)" Condition="!Exists('$(OutputPath)')" />
    <Message Text="Compiling with $(Compiler)..." Importance="high" />
    <Exec Command="$(Compiler) -c -std=$(CppVersion) -o $(OutputPath)%(CppSource.Filename).o %(CppSource.Identity)" />
  </Target>
  
  <Target Name="Link" DependsOnTargets="Compile">
    <Message Text="Linking with $(Compiler)..." Importance="high" />
    <Exec Command="$(Compiler) @(CppSource->'$(OutputPath)%(filename).o', ' ') -o $(OutputPath)$(OutputName)" />
  </Target>
  
  <Target Name="Clean">
    <Message Text="Cleaning..." Importance="high" />
    <Delete Files="$(OutputPath)/$(OutputName)" />
    <Delete Files="@(CppSource->'$(OutputPath)%(filename).o')" />
    <RemoveDir Directories="$(OutputPath)" />
  </Target>
  
  <Target Name="Rebuild" DependsOnTargets="Clean;Build">
    <Message Text="Building with $(Compiler)..." Importance="high" />
  </Target>
</Project>
```

In this example:
- `<PropertyGroup>` defines the properties `Compiler`, `CppVersion`, `OutputPath` and `OutputName`.
- `<ItemGroup>` contains the `<CppSource>` element, which includes all `.cpp` files in the project to be compiled.
- `<Target Name="Build">` defines the main build target. It depends on the `Link` target, which in turn depends on the `Compile` target. First, the `Compile` target compiles each of the files from `CppSource` one by one, then in the `Link` target, all object files are linked into the program.
- `<Target Name="Clean">` removes the compiled files.
- `<Target Name="Rebuild">` first executes `Clean`, and then `Build`.

### Basic Elements of a Project File

An MSBuild project file consists of four main parts:

*   **Properties:** Defined inside a `<PropertyGroup>` element. Properties are key-value pairs used to configure the build process, e.g., file paths, library versions, compiler flags. They can be treated like variables. Properties are processed in the order they appear in the project file, and their values can be overridden by redefining them.

*   **Items:** Defined inside an `<ItemGroup>` element. Items are lists of inputs for the build process, most often they are files.

*   **Tasks:** Tasks are units of executable code that MSBuild uses to perform build operations. Examples of tasks include `Csc` (running the C# compiler), `Copy` (copying files), `Message` (displaying a message).

*   **Targets:** Defined using the `<Target>` element. Targets group tasks into logical sequences. The `msbuild -targets` command displays a list of all available targets in the project. It is worth noting that targets have `Inputs` and `Outputs` attributes. These are used to implement incremental builds - MSBuild compares the modification dates of input and output files to decide if re-running the target is necessary.

### Referencing Properties and Items

In MSBuild files, to reference the values of defined properties and items, a special syntax is used:

*   **`$()` for Properties:** To get the value of a property, use its name inside parentheses `$(PropertyName)`. For example, `$(OutputName)` in the example above will be replaced by `program`.

*   **`@()` for Items:** To get a list of values from items, use the name of the item group inside parentheses `@(ItemGroupName)`. For example, `@(CppSource)` will be replaced by a list of all files (e.g., `main.cpp;log.cpp`).

### Metadata and Item Transformations

Each item in MSBuild, in addition to its value (e.g., a file path), can also have **metadata**. Metadata is additional information associated with a given item that can be defined and used in the build process.

#### Metadata Syntax: `%()`

To reference an item's metadata, the `%(MetadataName)` syntax is used. If we reference metadata within a target where a list of items is being processed (so-called "batching"), MSBuild iterates over each item and makes its metadata available.

**Example:**

Let's say we have a list of C++ files and we want to define a different language standard for each of them.

```xml
<ItemGroup>
  <CppSource Include="main.cpp">
    <LanguageStandard>c++20</LanguageStandard>
  </CppSource>
  <CppSource Include="log.cpp">
    <LanguageStandard>c++20</LanguageStandard>
  </CppSource>
  <CppSource Include="legacy.cpp">
    <LanguageStandard>c++11</LanguageStandard>
  </CppSource>
</ItemGroup>

<Target Name="Compile">
  <Message Text="Compiling @(CppSource) using standard %(CppSource.LanguageStandard)..." />
</Target>
```

In this example, `LanguageStandard` is custom metadata. When the `Compile` target is run, MSBuild will display:

```
Compiling main.cpp;log.cpp using standard c++20...
Compiling legacy.cpp using standard c++11...
```

#### Predefined Metadata

Each item has a set of predefined metadata, regardless of whether it was explicitly defined. Here are some of them:

*   **`%(Identity)`**: The value of the item itself (e.g., `main.cpp`).
*   **`%(Filename)`**: The file name without the extension (e.g., `main`).
*   **`%(Extension)`**: The file extension (e.g., `.cpp`).
*   **`%(FullPath)`**: The full, absolute path to the file.
*   **`%(RelativeDir)`**: The relative path to the directory containing the file.

A full list can be found in the [documentation](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-well-known-item-metadata).

#### Item Transformations

Transformations allow converting one list of items into another, using metadata. The transformation syntax is `'@(ItemGroup -> '%(Metadata)')'`. Optionally, we can also provide an alternative separator character (the default is ';'): `'@(ItemGroup -> '%(Metadata)', '_')'`.

**Example:**

Let's say we want to transform a list of `CppSource` source files into a list of `.o` object files.

```xml
<ItemGroup>
  <CppSource Include="main.cpp;utils.cpp" />
</ItemGroup>

<Target Name="ListObjectFiles">
  <Message Text="Object files: @(CppSource -> '%(Filename).o')" />
</Target>
```

In this case:

1.  `@(CppSource -> '%(Filename).o')` takes each item from `CppSource`.
2.  For each item, it gets the `%(Filename)` metadata (e.g., `main`, `utils`).
3.  It appends `.o` to it, creating a new list: `main.o;utils.o`.

The `ListObjectFiles` target will display: `Object files: main.o;utils.o`.

### Basic Commands

*   **Building a project:**
    ```bash
    msbuild <project_file_name>
    dotnet build <project_file_name>
    ```
    If there is only one project file in the directory, you can omit its name.

*   **Selecting a specific target:**
    ```bash
    msbuild <project_file_name> /t:<target_name>
    ```
    The `dotnet build` command does not have a direct switch for running custom targets. However, you can use the `dotnet msbuild` command for this purpose, which is part of the .NET SDK and works similarly to `msbuild`.
    ```bash
    msbuild <project_file_name> /t:<target_name>
    dotnet msbuild <project_file_name> /t:<target_name>
    ```
    For standard operations like `clean` or `publish`, it is recommended to use the dedicated `dotnet` commands:
    ```bash
    dotnet clean
    dotnet publish
    ```

*   **Passing properties:**
    Properties can be passed to `msbuild` and `dotnet build` using the `/p` switch (or `-p` and `--property` for `dotnet`).
    ```bash
    msbuild /p:Configuration=Release
    dotnet build -p:Configuration=Release
    dotnet build --property:Configuration=Release
    ```
    Many popular properties, such as `Configuration`, have shorter equivalents in `dotnet`:
    ```bash
    dotnet build -c Release
    ```
    All of the above commands will build the project in the `Release` configuration.

### Target Execution Order

MSBuild determines the order of target execution based on defined rules.

The order is as follows:

1.  **`InitialTargets` attribute:** Targets defined in this attribute of the `<Project>` element are run first, even if other targets were specified on the command line or in the `DefaultTargets` attribute.

2.  **Command-line targets:** If you run MSBuild with the `/t` switch (or `dotnet msbuild /t`), the specified targets will be executed after those from `InitialTargets`.

3.  **`DefaultTargets` attribute:** If no targets are specified on the command line, MSBuild will run the targets defined in this attribute of the `<Project>` element.

4.  **First target in the file:** If `InitialTargets`, `DefaultTargets` are not defined and no targets are specified on the command line, MSBuild will execute the first target it encounters in the project file.

After determining the initial targets, MSBuild uses the following attributes to recursively build the dependency tree and determine the final order:

*   **`DependsOnTargets`:** This attribute specifies that a given target depends on others. MSBuild will execute all targets from the `DependsOnTargets` list **before** executing the target that declares them.

*   **`BeforeTargets` and `AfterTargets`:** These attributes allow a target to be run before or after another, without modifying it.

It is worth remembering that **each target is executed only once** during a single build. Even if multiple targets declare a dependency on the same target, it will only be run on the first call.

Additionally, the **`Condition`** attribute on a target can cause it to be skipped if the condition is not met.

### `Condition` Attribute

The `Condition` attribute allows for the conditional execution of tasks/targets or the conditional definition of properties/items. **It can be attached to almost any node**, including:

*   `<PropertyGroup>` and `<Property>`
*   `<ItemGroup>` and individual `<Item>`
*   `<Target>`
*   `<Task>`
*   `<Import>`

For example:

```xml
<PropertyGroup>
  <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
</PropertyGroup>
```

In this case, if the `Configuration` property is not passed from the outside (e.g., from the command line), it will be assigned the value `Debug`.

### Predefined Tasks

MSBuild provides a multitude of built-in tasks that you can use in your targets. Here are a few examples:

*   **`Message`**: Displays a message in the build logs.
*   **`Copy`**: Copies files from one location to another.
*   **`Delete`**: Deletes files.
*   **`MakeDir`**: Creates directories.
*   **`Exec`**: Executes an external command or script.
*   **`Csc`**: Runs the C# compiler.
*   **`MSBuild`**: Runs other MSBuild projects, allowing for the building of dependencies.

A full list of built-in tasks with documentation can be found here: [MSBuild tasks](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-task-reference).

In addition to built-in tasks, you can also **create your own custom tasks**. This allows you to extend MSBuild with any logic needed in your build process. You can read about how to define your own tasks in the [documentation](https://learn.microsoft.com/en-us/visualstudio/msbuild/task-writing).

### Importing Other Files

MSBuild allows you to split build logic into multiple files using the `<Import>` element. This is key to maintaining order in large projects and is the basis for how SDK-style projects work.

```xml
<Project ...>
  ...
  <Import Project="Common.targets" />
</Project>
```

### SDK-style Projects

Modern .NET projects (since .NET Core) use a simplified format known as SDK-style projects. The `Sdk` attribute in the `<Project>` element automatically imports the appropriate `.props` and `.targets` files, which contain all the build logic.

In practice, the `Sdk` attribute is "syntactic sugar" for two imports. The notation:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  ...
</Project>
```

Is logically equivalent to manually importing the `.props` and `.targets` files from the SDK:

```xml
<Project>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />

  ...

  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
```

The first import (`Sdk.props`) is at the beginning of the file and loads default properties, and the second (`Sdk.targets`) is at the end to load targets and build logic.

The following example shows a typical project file, created with the command `dotnet new console -o ConsoleProject`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

This file, although much shorter, contains all the necessary logic to build a simple console application - it imports it from `Microsoft.NET.Sdk`. These imported files can be found in the .NET SDK installation directory. On Windows, this is typically `C:\Program Files\dotnet\sdk\[version]\Sdks\`, and on Linux `/usr/share/dotnet/sdk/[version]/Sdks/`.

### Predefined Items from the SDK

SDK-style projects automatically define many items that make work easier. Here are some of them:

*   **`Compile`**: Source code files to be compiled (by default, all `.cs` files in the project).
*   **`EmbeddedResource`**: Files to be embedded in the resulting assembly.
*   **`Content`**: Files that are not compiled but are to be copied to the output directory (e.g., configuration files, resources).
*   **`None`**: Files that are part of the project but do not participate in the build process (e.g., `README.md`).
*   **`ProjectReference`**: References to other projects.
*   **`PackageReference`**: References to NuGet packages.

A full list of popular items can be found here: [Common MSBuild project items](https://learn.microsoft.com/visualstudio/msbuild/common-msbuild-project-items).

### Logging and Diagnosing Problems

MSBuild offers advanced logging options that are invaluable when diagnosing build problems.

*   **Log verbosity:**
    ```bash
    msbuild /v:detailed
    dotnet build --verbosity detailed
    ```
    Possible values are `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, and `diag[nostic]`.

*   **Logging to a file:**
    ```bash
    msbuild /flp:LogFile=build.log;Verbosity=diagnostic
    dotnet build /flp:LogFile=build.log;Verbosity=diagnostic
    ```
    This command will save the logs to the `build.log` file.