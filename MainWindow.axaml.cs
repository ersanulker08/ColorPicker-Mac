using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ColorPickerApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Project> _projects;
    private Project? _current;

    private readonly DispatcherTimer _timer;
    private bool _picking;
    private bool _wasLeftDown;

    public MainWindow()
    {
        InitializeComponent();

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

        ProjectList.SelectedIndex = 0;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        _timer.Tick += (_, _) => PollMouse();
        _timer.Start();
    }

    private void OnProjectSelected(object? sender, SelectionChangedEventArgs e)
    {
        _current = ProjectList.SelectedItem as Project;
        ProjectNameBox.Text = _current?.Name ?? "";
        ColorList.ItemsSource = _current?.Colors;
    }

    private void OnProjectNameChanged(object? sender, TextChangedEventArgs e)
    {
        if (_current == null) return;
        _current.Name = ProjectNameBox.Text ?? "";
        Save();
    }

    private void OnAddProject(object? sender, RoutedEventArgs e)
    {
        var project = new Project { Name = "Yeni Proje" };
        _projects.Add(project);
        ProjectList.SelectedItem = project;
        Save();
    }

    private void OnDropperClick(object? sender, RoutedEventArgs e)
    {
        _picking = true;
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    private void PollMouse()
    {
        bool isLeftDown = Native.CGEventSourceButtonState(0, 0);

        if (_picking && isLeftDown && !_wasLeftDown)
        {
            IntPtr eventRef = Native.CGEventCreate(IntPtr.Zero);
            CGPoint location = Native.CGEventGetLocation(eventRef);
            Native.CFRelease(eventRef);

            var (r, g, b) = Native.ReadPixel(location.X, location.Y);
            string hex = Native.ToHex(r, g, b);

            _current?.Colors.Add(new ColorEntry { Name = hex, Hex = hex, R = r, G = g, B = b });

            _picking = false;
            Cursor = Cursor.Default;
            Save();
        }

        _wasLeftDown = isLeftDown;
    }

    private void Save()
    {
        ProjectStore.Save(_projects);
    }
}
