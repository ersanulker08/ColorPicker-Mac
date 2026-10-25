using Avalonia.Controls;

namespace ColorPickerApp;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // Önce mevcut değerleri göster, sonra olayları bağla ki açılışta gereksiz kayıt olmasın.
        AlwaysOnTopSwitch.IsChecked = AppSettings.Current.AlwaysOnTop;
        LanguageBox.SelectedIndex = AppSettings.Current.Language == "tr" ? 1 : 0;

        AlwaysOnTopSwitch.IsCheckedChanged += (_, _) =>
        {
            AppSettings.Current.AlwaysOnTop = AlwaysOnTopSwitch.IsChecked == true;
            AppSettings.Save();
        };

        LanguageBox.SelectionChanged += (_, _) =>
        {
            string language = LanguageBox.SelectedIndex == 1 ? "tr" : "en";
            AppSettings.Current.Language = language;
            Loc.Instance.Language = language;
            AppSettings.Save();
        };

        CloseButton.Click += (_, _) => Close();
    }
}
