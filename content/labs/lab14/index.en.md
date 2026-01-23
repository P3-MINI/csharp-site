---
title: "Lab14"
weight: 10
---

# Laboratory 14: Interoperability, Marshalling, Unsafe context

## Safe handles

Create a program reading files using external functions from native system libraries.

### Starting code

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab14/student/SafeHandles" >}}

### Task description

Caution is required when interacting with code that is not [managed](https://csharp.mini.pw.edu.pl/en/lectures/01.introduction/#main-features-of-the-language) - e.g. base C++, C. Safety measures provided by the .NET environment stop guaranteeing protection the moment code calls into external code. Structures created in unmanaged code are often represented by pointers or other identifiers that have unique values reserved for invalid states (commonly 0 or -1). Use classes inheriting from `SafeHandle` to make sure that resources you requested are valid before use.

A partially implemented class that imports system functions for handling files can be found in the `FileInteraction` project - in `WindowsFile.cs` or `UnixFile.cs` depending which operating system you use. Only the relevant one will be used for compilation. Take a look at the project file `FileInteraction.csproj` - conditional exclusion of the file that doesn't match your system is implemented for you.

Check what values the file opening function declares as invalid. Make the `MyFile` class inherit from the correct `SafeHandle` so it matches the invalid values. Make sure you do it in `WindowsFile.cs` if you're on Windows or `UnixFile.cs` if you're on OSX or Linux. Finish the correct implementation for your system. Fill in `Open` and `Read` in the file `MyFile.cs`. You can test the program by running it.

After the workshop you're encouraged to try this task for the other system setup.

{{% hint info %}}
**Useful links:**

- [Microsoft Learn: MSBuild conditions](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-conditions)
- [Microsoft Learn: SafeHandles namespace](https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.safehandles?view=net-9.0)
- [POSIX Manual: open](https://www.man7.org/linux/man-pages/man3/open.3p.html)
- [Microsoft Learn: CreateFile](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilea)


{{% /hint %}}


### Example solution

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab14/solution/SafeHandles" >}}

## Binding and Marshalling

Use an unmanaged library to create visual patterns.

### Starting code

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab14/student/Binding" >}}

### Task description

Import the functions shared in `pattern.h` using `LibraryImport` in the `PatternGeneration` project, `NativeMethods.cs`. Declare all required classes and structures. Implement wrapping functions in the `Pattern` class (`Pattern.cs`) so that the library user only has to interact with managed functions.
- Some arguments might require the `[In]` attribute to specify the direction of information exchange. Similarily, if there was a change in an argument that needed to be shown back to the caller, the `[Out]` tag would be appropriate.
- Structures on the managed side of the interaction need to be laid out precisely like the ones on the unmanaged side. Alternatively, one can define custom marshalling. That requires the programmer to define rules for turning the unmanaged structures into their managed counterparts and vice versa using the [`CustomMarshaller`](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/tutorial-custom-marshaller) attribute.
- Treat the `pattern_t` structure as opaque. Simply treat its pointer as one of the `SafeHandles` and don't worry about transfering data. Use the handle itself for calling the other imported functions, especially after you implement the inheritance.

The library distributors might not release the full internal data layout for the user. The user might care about only a small part of the structure and not need it whole. Use `Marshall.Copy` in the `GetImage` function (`Pattern.cs`) to extract the dimensions and contents of the `color_t[] values` array hidden under the opaque handle. Utilise the fact that the fields will be laid out sequentially like in `pattern.h` (with offset 0, then offset `sizeof(int)` and `sizeof(int)*2`). The struct `color_t` has the same layout as the `Rgb24` structure. Use the acquired data to create an image with  `Image.LoadPixelData<Rgb24>`.

Fill in the `Main` function in the `PatternGenerationDemo` project to create at least one image each with the functions `pattern_enstripen` and `pattern_populate`.

Uzupełnij funkcję `Main` w projekcie `PatternGenerationDemo` aby tworzyło się co najmniej po jednym obrazie z użyciem fukcji `pattern_enstripen` oraz `pattern_populate`.

{{% hint info %}}
**Useful links:**

- [Microsoft Learn: P/Invoke](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation)
- [Microsoft Learn: Struct layout](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute?view=net-9.0)
- [Microsoft Learn: Type marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling)
- [Microsoft Learn: Built-in type marshalling](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.disableruntimemarshallingattribute?view=net-9.0)


{{% /hint %}}


### Example solution

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab14/solution/Binding" >}}