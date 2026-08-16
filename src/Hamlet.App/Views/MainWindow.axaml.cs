using Avalonia.Controls;
using Avalonia.Input;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

/// <summary>
/// The main window. Its code-behind owns only the two facts the view knows and
/// the ViewModel cannot: which keys were pressed, and whether anybody is
/// looking at it.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>Creates the window and wires the view-only facts.</summary>
    public MainWindow()
    {
        InitializeComponent();

        // Arrow keys = ±10 Hz, the headphone-tuning path (HM-DEC-015).
        AddHandler(KeyDownEvent, OnTuneKey, handledEventsToo: false);

        // The feed pauses when nobody is looking (HM-DEC-020). Visibility is
        // the view's fact, so the view pushes it; the ViewModel owns what to
        // do about it.
        PropertyChanged += OnWindowPropertyChanged;
        Opened += (_, _) =>
        {
            PushVisibility();
            StartReconnect();
            WatchTheCanvasSize();
        };

        // A LAST SAVE ON THE WAY OUT (HM-DEC-089). Every change to the canvas
        // already saves as it happens, so this is a backstop rather than the
        // mechanism, and a backstop is worth having for the one thing the
        // operator would have to rebuild by hand.
        Closing += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.KeepTheCanvas();
            }
        };
    }

    /// <summary>
    /// Tell the canvas how much of it can actually be seen (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// <para>**HOW BIG THE VIEWPORT IS, IS A VIEW FACT**, and this is the same
    /// division the key handler and the visibility push already live by: the view
    /// knows the size, the view model decides what to do about it.</para>
    /// <para>It fires on every layout pass, and the view model only acts when
    /// something is genuinely out of reach, so the common case costs a
    /// comparison.</para>
    /// </remarks>
    private void WatchTheCanvasSize()
    {
        if (this.FindControl<ScrollViewer>("CanvasView") is not { } view)
        {
            return;
        }

        void Fit()
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Canvas.FitInto(view.Viewport.Width, view.Viewport.Height);
            }
        }

        view.PropertyChanged += (_, e) =>
        {
            if (e.Property == ScrollViewer.ViewportProperty)
            {
                Fit();
            }
        };

        Fit();
    }

    /// <summary>
    /// Kicks off the startup reconnect without waiting for it (HM-DEC-052).
    /// </summary>
    /// <remarks>
    /// Deliberately not awaited. Opening a COM port and waiting for a radio to
    /// answer takes as long as it takes, and a window that will not paint until
    /// the radio replies is a window that looks broken to anybody whose rig is
    /// switched off. So the window comes up first and the status line fills in
    /// afterwards.
    /// </remarks>
    private void StartReconnect()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            _ = vm.ReconnectOnStartupAsync();
        }
    }

    private void OnWindowPropertyChanged(
        object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty || e.Property == IsVisibleProperty)
        {
            PushVisibility();
        }
    }

    private void PushVisibility()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetWindowVisible(IsVisible && WindowState != WindowState.Minimized);
        }
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
