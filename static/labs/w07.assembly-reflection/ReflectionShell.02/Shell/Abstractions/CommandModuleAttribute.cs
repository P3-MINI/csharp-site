namespace Shell.Abstractions;

// Marks a class whose [Command] methods are discovered automatically
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CommandModuleAttribute : Attribute;
