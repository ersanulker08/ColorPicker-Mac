using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ColorPickerApp;

public static class ProjectStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ColorPicker",
        "projects.json");

    public static void Save(IEnumerable<Project> projects)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        string json = JsonSerializer.Serialize(projects, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    public static List<Project> Load()
    {
        if (!File.Exists(FilePath)) return new List<Project>();

        try
        {
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Project>>(json) ?? new List<Project>();
        }
        catch
        {
            return new List<Project>();
        }
    }
}