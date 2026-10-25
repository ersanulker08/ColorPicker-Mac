using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ColorPickerApp;

public class Project : INotifyPropertyChanged
{
    private string _name = Loc.Instance["NewProject"];
    private ObservableCollection<ColorEntry> _colors;

    public Project()
    {
        _colors = new ObservableCollection<ColorEntry>();
        _colors.CollectionChanged += OnColorsChanged;
    }

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

    public ObservableCollection<ColorEntry> Colors
    {
        get => _colors;
        set
        {
            _colors.CollectionChanged -= OnColorsChanged;
            _colors = value;
            _colors.CollectionChanged += OnColorsChanged;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorCount)));
        }
    }

    // Proje listesinde isminin yanında gösterilen renk sayısı. Dosyaya kaydedilmez.
    [JsonIgnore]
    public int ColorCount => _colors.Count;

    private void OnColorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorCount)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
