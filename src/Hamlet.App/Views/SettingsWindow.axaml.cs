using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Hamlet.App.Views;

/// <summary>
/// The Settings dialog. Every switch writes straight through to the settings
/// file, so there is no Apply button and nothing to lose by closing it
/// (HM-DEC-018).
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>Creates the dialog.</summary>
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
