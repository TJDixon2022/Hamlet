using Avalonia.Controls;
using Avalonia.Interactivity;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

/// <summary>
/// The About box: version, runtime, session id and a copy-diagnostics button
/// (HM-DEC-019). It is §0.0.1 meeting the user.
/// </summary>
public partial class AboutWindow : Window
{
    /// <summary>Creates the dialog.</summary>
    public AboutWindow()
    {
        InitializeComponent();
    }

    /// <summary>The clipboard lives on the TopLevel, not the ViewModel, so
    /// the copy runs here and reports back.</summary>
    private async void OnCopyDiagnosticsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AboutViewModel vm)
        {
            return;
        }

        try
        {
            var clipboard = GetTopLevel(this)?.Clipboard;
            if (clipboard is null)
            {
                vm.ReportCopyFailed();
                return;
            }

            await clipboard.SetTextAsync(vm.DiagnosticsText);
            vm.ReportCopied();
        }
        catch (Exception)
        {
            vm.ReportCopyFailed();
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
