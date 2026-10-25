using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ColorPickerApp;

public class Project : INotifyPropertyChanged
{
    private string _name = "Yeni Proje";

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public ObservableCollection<ColorEntry> Colors { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
}