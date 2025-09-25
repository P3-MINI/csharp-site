using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MyTask;

public class CreateFileTask : Task
{
    [Required]
    public required string Filename { get; set; }

    public override bool Execute()
    {
        try
        {
            File.WriteAllText(Filename, "Hello, custom task!");
            Log.LogMessage(MessageImportance.High, $"File {Filename} with greeting created successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to create file {Filename}:\n {ex.Message}");
            return false;
        }
    }
}