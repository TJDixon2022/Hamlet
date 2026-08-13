using Avalonia.Controls;
using Avalonia.Input;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Arrow keys = ±10 Hz, the headphone-tuning path (HM-DEC-015).
        AddHandler(KeyDownEvent, OnTuneKey, handledEventsToo: false);
    }

    private void OnTuneKey(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var delta = e.Key switch
        {
            Key.Right or Key.Up => 10,
            Key.Left or Key.Down => -10,
            _ => 0,
        };

        if (delta != 0)
        {
            vm.FrequencyHz += delta;
            e.Handled = true;
        }
    }
}
