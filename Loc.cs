using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ColorPickerApp;

// Çeviri tablosu: anahtar -> (İngilizce, Türkçe). Dil değişince arayüz anında güncellenir.
public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    private static readonly Dictionary<string, (string En, string Tr)> Table = new()
    {
        ["SelectedProject"] = ("SELECTED PROJECT", "SEÇİLİ PROJE"),
        ["AllProjects"] = ("ALL PROJECTS", "TÜM PROJELER"),
        ["ProjectNamePlaceholder"] = ("Project name", "Proje adı"),
        ["NewProject"] = ("New Project", "Yeni Proje"),
        ["DeleteProject"] = ("Delete project", "Projeyi sil"),
        ["DropperTip"] = ("Eyedropper: pick a color from the screen", "Damlalık: ekrandan renk seç"),
        ["Hint"] = ("Press the eyedropper, then click anywhere on screen.", "Damlalığa bas, sonra ekranda bir yere tıkla."),
        ["HintPicking"] = ("Now click anywhere on screen… (Esc: cancel)", "Şimdi ekranda bir yere tıkla… (Esc: vazgeç)"),
        ["Empty"] = ("No colors in this project yet.\nPress the eyedropper, then click anywhere on screen.",
                     "Bu projede henüz renk yok.\nDamlalığa bas, sonra ekranda bir yere tıkla."),
        ["Copy"] = ("Copy", "Kopyala"),
        ["DeleteColor"] = ("Delete color", "Rengi sil"),
        ["Copied"] = ("Copied: {0}", "Kopyalandı: {0}"),
        ["ColorOne"] = ("1 COLOR", "1 RENK"),
        ["ColorMany"] = ("{0} COLORS", "{0} RENK"),
        ["Settings"] = ("Settings", "Ayarlar"),
        ["AlwaysOnTop"] = ("Always on top", "Her zaman üstte"),
        ["AlwaysOnTopDesc"] = ("Keep this window above other apps.", "Bu pencere diğer uygulamaların üstünde kalır."),
        ["Language"] = ("Language", "Dil"),
        ["Close"] = ("Close", "Kapat"),
        ["Confirm"] = ("Confirm", "Onay"),
        ["Cancel"] = ("Cancel", "Vazgeç"),
        ["Delete"] = ("Delete", "Sil"),
        ["ConfirmProject"] = ("Delete \"{0}\"?", "\"{0}\" projesi silinsin mi?"),
        ["ConfirmProjectColors"] = ("Delete \"{0}\" and its {1} colors?", "\"{0}\" projesi ve içindeki {1} renk silinsin mi?"),
    };

    private string _language = "en";

    public string Language
    {
        get => _language;
        set
        {
            string normalized = value == "tr" ? "tr" : "en";
            if (_language == normalized) return;

            _language = normalized;
            // "Item[]" bildirimi, XAML'deki tüm [anahtar] bağlamalarını yeniden okutur.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            LanguageChanged?.Invoke();
        }
    }

    public string this[string key]
    {
        get
        {
            if (!Table.TryGetValue(key, out var pair)) return key;
            return _language == "tr" ? pair.Tr : pair.En;
        }
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(this[key], args);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Kod tarafında güncellenmesi gereken yazılar için (ipucu metni, sayaç vb.).
    public event Action? LanguageChanged;
}
