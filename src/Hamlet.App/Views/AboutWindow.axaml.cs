using Avalonia.Controls;
using Avalonia.Interactivity;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

public partial class AboutWindow : Window
{
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
