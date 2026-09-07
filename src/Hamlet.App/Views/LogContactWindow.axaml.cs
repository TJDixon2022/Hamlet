using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

/// <summary>The Log dialog.</summary>
/// <remarks>
/// <para>**IT WRITES NOTHING AND IT TRANSMITS NOTHING.** Save records that Save
/// was pressed and closes; the caller is what appends to the file, and there is no
/// route from here into the send path at all.</para>
/// <para>**THE TWO BUTTONS ARE CODE-BEHIND BECAUSE CLOSING A WINDOW IS A VIEW
/// FACT**, the same reasoning the decoded row's context handler is in code-behind
/// (§0.1). The view model knows whether Save was pressed and knows nothing about
/// windows.</para>
/// </remarks>
public partial class LogContactWindow : Window
{
    /// <summary>Creates the window.</summary>
    public LogContactWindow() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LogContactViewModel model)
        {
            model.SaveCommand.Execute(null);
        }

        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();
}
