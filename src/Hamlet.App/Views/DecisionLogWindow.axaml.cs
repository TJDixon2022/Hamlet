using Avalonia.Controls;
using Avalonia.Interactivity;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

/// <summary>
/// What Hamlet has recently decided (HM-DEC-077).
/// </summary>
/// <remarks>
/// The companion to "What the radio is doing". That window answers what the
/// radio is doing; this one answers what Hamlet did about it, which is the half
/// that was missing on the evening a disabled button explained nothing.
/// </remarks>
public partial class DecisionLogWindow : Window
{
    /// <summary>Creates the window.</summary>
    public DecisionLogWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// The clipboard lives on the TopLevel, so the copy runs here. Same
    /// arrangement the rig window and the About box use.
    /// </summary>
    private async void OnCopy(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not DecisionLogViewModel vm)
        {
            return;
        }

        try
        {
            var clipboard = GetTopLevel(this)?.Clipboard;

            if (clipboard is not null)
            {
                await clipboard.SetTextAsync(vm.ForBugReport());
            }
        }
        catch (Exception)
        {
            // A clipboard that refuses is never worth taking a window down for
            // (§8). The text is on screen either way.
        }
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
