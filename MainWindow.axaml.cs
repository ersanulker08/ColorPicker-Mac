using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace ColorPickerApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Project> _projects;
    private Project? _current;
    private ObservableCollection<ColorEntry>? _watchedColors;

    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _toastTimer;
    private readonly Cursor _dropperCursor;
    private bool _picking;
    private bool _waitRelease;
    private bool _wasLeftDown;

    public MainWindow()
    {
        // Kayıtlı ayarları pencere çizilmeden önce uygula (dil ve "her zaman üstte").
        Loc.Instance.Language = AppSettings.Current.Language;

        InitializeComponent();

        Topmost = AppSettings.Current.AlwaysOnTop;
        AppSettings.Changed += () => Topmost = AppSettings.Current.AlwaysOnTop;
        Loc.Instance.LanguageChanged += RefreshTexts;

        _dropperCursor = BuildDropperCursor() ?? new Cursor(StandardCursorType.Cross);
        RefreshTexts();

        _projects = new ObservableCollection<Project>(ProjectStore.Load());
        if (_projects.Count == 0)
        {
            _projects.Add(new Project());
        }

        ProjectList.ItemsSource = _projects;

        ProjectList.SelectionChanged += OnProjectSelected;
        ProjectNameBox.TextChanged += OnProjectNameChanged;
        AddProjectButton.Click += OnAddProject;
        DropperButton.Click += OnDropperClick;
        SettingsButton.Click += (_, _) => new SettingsWindow().ShowDialog(this);

        ProjectList.SelectedIndex = 0;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        _timer.Tick += (_, _) => PollMouse();
        _timer.Start();

        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1600) };
        _toastTimer.Tick += (_, _) =>
        {
            _toastTimer.Stop();
            ToastBorder.IsVisible = false;
        };
    }

    // ---------- Projeler ----------

    private void OnProjectSelected(object? sender, SelectionChangedEventArgs e)
    {
        _current = ProjectList.SelectedItem as Project;
        ProjectNameBox.Text = _current?.Name ?? "";
        ColorList.ItemsSource = _current?.Colors;

        // Seçili projenin renk listesindeki ekleme/silmeleri izle (boş ekran yazısı ve sayaç için).
        if (_watchedColors != null)
        {
            _watchedColors.CollectionChanged -= OnColorsChanged;
        }

        _watchedColors = _current?.Colors;
        if (_watchedColors != null)
        {
            _watchedColors.CollectionChanged += OnColorsChanged;
        }

        RefreshColorInfo();
    }

    private void OnColorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshColorInfo();
    }

    private void RefreshColorInfo()
    {
        int count = _current?.Colors.Count ?? 0;
        EmptyText.IsVisible = count == 0;
        CountText.Text = count == 1 ? Loc.Instance["ColorOne"] : Loc.Instance.Format("ColorMany", count);
    }

    // Dil değişince kod tarafından yazılan metinleri yenile.
    private void RefreshTexts()
    {
        HintText.Text = Loc.Instance[_picking ? "HintPicking" : "Hint"];
        RefreshColorInfo();
    }

    private void OnProjectNameChanged(object? sender, TextChangedEventArgs e)
    {
        if (_current == null) return;
        _current.Name = ProjectNameBox.Text ?? "";
        Save();
    }

    private void OnAddProject(object? sender, RoutedEventArgs e)
    {
        var project = new Project { Name = Loc.Instance["NewProject"] };
        _projects.Add(project);
        ProjectList.SelectedItem = project;
        Save();
    }

    // Sağ tıklayınca o proje seçilsin ki menü hangi projeye ait olduğu belli olsun.
    private void OnProjectPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(null).Properties.IsRightButtonPressed
            && sender is Control { DataContext: Project project })
        {
            ProjectList.SelectedItem = project;
        }
    }

    private async void OnDeleteProjectClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: Project project }) return;

        string message = project.Colors.Count == 0
            ? Loc.Instance.Format("ConfirmProject", project.Name)
            : Loc.Instance.Format("ConfirmProjectColors", project.Name, project.Colors.Count);

        if (!await ConfirmWindow.Ask(this, message)) return;

        int index = _projects.IndexOf(project);
        bool wasSelected = ReferenceEquals(project, _current);

        _projects.Remove(project);
        if (_projects.Count == 0)
        {
            _projects.Add(new Project());
        }

        if (wasSelected)
        {
            ProjectList.SelectedIndex = Math.Min(index, _projects.Count - 1);
        }

        Save();
    }

    // ---------- Damlalık ----------

    private void OnDropperClick(object? sender, RoutedEventArgs e)
    {
        if (_picking) return;

        _picking = true;
        _waitRelease = true;   // damlalığa basan tıklama renk sayılmasın, sonraki tıklama sayılsın
        DropperButton.IsEnabled = false;
        RefreshTexts();
        Cursor = _dropperCursor;
    }

    private void StopPicking()
    {
        _picking = false;
        DropperButton.IsEnabled = true;
        RefreshTexts();
        Cursor = Cursor.Default;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && _picking)
        {
            StopPicking();
            e.Handled = true;
        }
    }

    private void PollMouse()
    {
        bool isLeftDown = Native.CGEventSourceButtonState(0, 0);

        if (_picking)
        {
            if (_waitRelease)
            {
                // Fare tuşu bırakılana kadar bekle, böylece sadece bir sonraki tıklama okunur.
                if (!isLeftDown) _waitRelease = false;
            }
            else if (isLeftDown && !_wasLeftDown)
            {
                IntPtr eventRef = Native.CGEventCreate(IntPtr.Zero);
                CGPoint location = Native.CGEventGetLocation(eventRef);
                Native.CFRelease(eventRef);

                var (r, g, b) = Native.ReadPixel(location.X, location.Y);
                string hex = Native.ToHex(r, g, b);

                if (_current != null)
                {
                    _current.Colors.Add(new ColorEntry { Name = NextColorName(_current), Hex = hex, R = r, G = g, B = b });
                    ColorList.ScrollIntoView(_current.Colors.Count - 1);
                }

                StopPicking();
                Save();
            }
        }

        _wasLeftDown = isLeftDown;
    }

    // Color1, Color2, ... şeklinde, projede henüz kullanılmayan ilk ismi bulur.
    private static string NextColorName(Project project)
    {
        int n = 1;
        while (project.Colors.Any(c => c.Name == $"Color{n}"))
        {
            n++;
        }

        return $"Color{n}";
    }

    // Damlalık ikonunu çizip fare imlecine çevirir. Uç noktası (sol alt) tıklanan pikseldir.
    private static Cursor? BuildDropperCursor()
    {
        try
        {
            if (Application.Current is null ||
                !Application.Current.TryGetResource("DropperIcon", null, out object? res) ||
                res is not Geometry geometry)
            {
                return null;
            }

            Path MakePath(IBrush stroke, double thickness) => new()
            {
                Data = geometry,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeLineCap = PenLineCap.Round,
                StrokeJoin = PenLineJoin.Round,
                Stretch = Stretch.Uniform,
                Width = 22,
                Height = 22,
                RenderTransformOrigin = RelativePoint.Center,
                RenderTransform = new RotateTransform(45)
            };

            // Altta kalın siyah kontur, üstte beyaz çizgi: her arka planda görünsün.
            var host = new Grid { Width = 32, Height = 32 };
            host.Children.Add(MakePath(Brushes.Black, 4));
            host.Children.Add(MakePath(Brushes.White, 1.8));
            host.Measure(new Size(32, 32));
            host.Arrange(new Rect(0, 0, 32, 32));

            var bitmap = new RenderTargetBitmap(new PixelSize(32, 32), new Vector(96, 96));
            bitmap.Render(host);

            return new Cursor(bitmap, new PixelPoint(8, 24));
        }
        catch
        {
            return null;
        }
    }

    // ---------- Renk satırı ----------

    // İsim kutusu değişince otomatik kaydeder.
    private void OnColorNameChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { DataContext: ColorEntry entry } box)
        {
            entry.Name = box.Text ?? "";
            Save();
        }
    }

    // Satırın sağındaki ✕ düğmesi: o rengi projeden siler.
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ColorEntry entry })
        {
            _current?.Colors.Remove(entry);
            Save();
        }
    }

    // HEX / R / G / B yanındaki düğmeler: değeri panoya kopyalar.
    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: not null } button && Clipboard is { } clipboard)
        {
            string text = button.Tag.ToString() ?? "";
            await clipboard.SetTextAsync(text);
            ShowToast(Loc.Instance.Format("Copied", text));
        }
    }

    private void ShowToast(string text)
    {
        ToastText.Text = text;
        ToastBorder.IsVisible = true;
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void Save()
    {
        ProjectStore.Save(_projects);
    }
}
