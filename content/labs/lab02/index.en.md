---
title: "Lab02"
weight: 10
---

# Lab 2: MSBuild, Unit Tests, .NET CLI

## Task 1: MSBuild

Your task is to modify the `CppProject.proj` file to add the following functionalities:

### 1. `Debug` and `Release` Configuration

The goal is to add support for two build configurations to the project: `Debug` and `Release`.
- **Debug**: The development configuration should include debugging symbols and have optimizations disabled.
- **Release**: The production configuration should be optimized for performance and not include debugging symbols.

**Requirements:**
- Add a `Configuration` property that will be set to `Debug` by default.
- Use conditional property groups (`PropertyGroup`) to define different compiler flags (for debugging symbols and optimizations) depending on the active configuration.
- Modify the targets to use the defined flags; `-O0` and `-g` for the `Debug` configuration; `-O2` for the `Release` configuration.
- Modify the `OutputPath` so that the output files for each configuration go into separate subdirectories (e.g., `build/Debug/` and `build/Release/`).

When you are finished, build the application from the command line in both the development and production configurations.

### 2. Incremental Builds

To speed up the build process, incremental compilation is often implemented. This means that only files that have changed since the last compilation should be recompiled. MSBuild accomplishes this by comparing the timestamps of the files defined in the `Inputs` and `Outputs` attributes of a given target. If all output files (`Outputs`) are newer than all input files (`Inputs`), MSBuild skips the execution of that target, saving time.

**Requirements:**
- Use the `Inputs` and `Outputs` attributes in the `Compile` and `Link` targets.
- The `Compile` target for a given `.cpp` file should only run if the `.cpp` file itself or any of the header files (`.h`) in the project is newer than its corresponding object file (`.o`).
- The `Link` target should only run if any of the object files are newer than the executable file.

Clean the project with the `Clean` target, then build it twice with the `Build` target.

### 3. Creating a Distribution Package

The goal is to automate the creation of a `.zip` package containing the finished application and additional files.

**Requirements:**
- Add a new target to the project named `CreateDist`.
- The `CreateDist` target should depend on the `Build` target.
- Add a `Version` property (e.g., `1.0.0`) to be used in the package name.
- Add an item group where you define a `DistFiles` item containing `README.md` and `LICENSE`.
- In the `CreateDist` target:
    - Copy the executable file and the distribution files to a temporary directory.
    - Use the `ZipDirectory` task (https://learn.microsoft.com/visualstudio/msbuild/zipdirectory-task) to pack the contents of the temporary directory.
    - The name of the resulting archive file should have the format `$(OutputName)-$(Version).zip`.
    - Delete the temporary folder.

Call the `CreateDist` target for the `Release` configuration.

### Sample Solution

A sample solution can be found in the [CppProject.proj](/labs/lab02/solution/CppProject/CppProject.proj) file.

## Task 2: .NET SDK

> TODO
