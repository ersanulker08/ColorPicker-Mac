using System;
using System.IO;
using System.Text.Json;

namespace ColorPickerApp;

// Kullanıcının seçtiği ayarlar. settings.json olarak projelerin yanına kaydedilir.
public class AppSettings
{
    public bool AlwaysOnTop { get; set; } = true;
    public string Language { get; set; } = "en";

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ColorPicker",
        "settings.json");

    public static AppSettings Current { get; private set; } = Load();

    // Ayarlar kaydedilince tetiklenir; ana pencere "üstte kal" ayarını buradan günceller.
    public static event Action? Changed;

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                if (settings != null)
                {
                    if (settings.Language != "tr") settings.Language = "en";
                    return settings;
                }
            }
        }
        catch
        {
            // Bozuk dosya olursa varsayılan ayarlarla devam et.
        }

        return new AppSettings();
    }

    public static void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
        Changed?.Invoke();
    }
}
