---
title: "Access Modifiers"
weight: 40
---

# Access Modifiers

* **public** - accessible from everywhere; default for **members of enums and interfaces**
* **internal** - accessible only from within the *assembly* - i.e., inside the files that create a `.dll` or `.exe` file; default for **non-nested types**
* **private** - accessible only within the type; default for **members of classes and structs**
* **protected** - accessible within the type and its subclasses
* **protected internal** - the union of **protected** and **internal**
* **private protected** - the intersection of **protected** and **internal**
* **file** (C# 11) - accessible only from within the same file, applicable only to type declarations.
