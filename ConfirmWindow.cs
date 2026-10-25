using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ColorPickerApp;

// "Emin misin?" diye soran küçük onay penceresi. Silmeden önce kullanılır.
public class ConfirmWindow : Window
{
    private ConfirmWindow(string message, string confirmText)
    {
        Title = Loc.Instance["Confirm"];
        Width = 380;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var cancelButton = new Button { Content = Loc.Instance["Cancel"] };
        var confirmButton = new Button { Content = confirmText };
        confirmButton.Classes.Add("danger");

        cancelButton.Click += (_, _) => Close(false);
        confirmButton.Click += (_, _) => Close(true);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { cancelButton, confirmButton }
        };

        Content = new StackPanel
        {
            Margin = new Thickness(22),
            Spacing = 20,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                buttons
            }
        };
    }

    public static Task<bool> Ask(Window owner, string message)
    {
        return new ConfirmWindow(message, Loc.Instance["Delete"]).ShowDialog<bool>(owner);
    }
}
