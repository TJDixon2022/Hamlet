using Avalonia.Controls;
using Avalonia.Interactivity;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

/// <summary>
/// Everything Hamlet knows about the radio, on one screen (HM-DEC-050).
/// </summary>
/// <remarks>
/// The screen that would have answered half an hour of walking to the radio and
/// reading menu settings out loud. It is also what a bug report should carry,
/// which is what the copy button is for.
/// </remarks>
public partial class RigDiagnosticsWindow : Window
{
    /// <summary>Creates the window.</summary>
    public RigDiagnosticsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The clipboard lives on the TopLevel rather than the ViewModel, so the
    /// copy runs here. Same arrangement the About box uses.
    /// </summary>
    private async void OnCopy(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not RigDiagnosticsViewModel vm)
        {
            return;
        }

        try
        {
            var clipboard = GetTopLevel(this)?.Clipboard;

            if (clipboard is not null)
            {
                await clipboard.SetTextAsync(vm.CopyText);
            }
        }
        catch (Exception)
        {
            // A clipboard that refuses is not worth taking the window down for
            // (§8). The text is on screen either way.
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
