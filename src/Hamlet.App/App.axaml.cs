using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App;

/// <summary>
/// The application shell: loads settings, opens the telemetry writer, and puts
/// the main window on screen with its state restored.
/// </summary>
public partial class App : Application
{
    private AppSettings _settings = new();
    private JsonlTelemetry? _telemetry;

    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _settings = SettingsStore.Load();

            var version = Assembly.GetExecutingAssembly()
                .GetName().Version?.ToString(3) ?? "0.0.0";
            _telemetry = new JsonlTelemetry(
                SettingsStore.TelemetryFolder,
                version,
                category => _settings.IsTelemetryEnabled(category),
                _settings.TelemetryMaxMegabytes * 1024L * 1024L);
            AppEvents.AppStart(_telemetry);

            // **ONE PICTURE OF WHAT STATE THIS MACHINE IS IN** (work instruction 304
            // task 2, Tim's ruling of 2026-09-10: *telemetry should be able to
            // diagnose any issue*). It is written here, as early as there is a sink
            // and settings to describe, and **before the window is built** - so a
            // machine broken enough that the window never appears has still said what
            // was wrong with it.
            //
            // **THE RADIO, THE CLOCK AND READINESS ARE NOT KNOWN YET AT THIS POINT**
            // and the snapshot says `unknown` with a reason for each rather than
            // leaving the field out. Task 3's state-change events carry them the
            // moment they become true.
            StartupFacts.Write(
                _telemetry,
                _settings,
                categoriesOn: category => _settings.IsTelemetryEnabled(category));

            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(_settings, _telemetry),
            };

            RestoreWindowState(window);
            window.Closing += (_, _) =>
            {
                SaveWindowState(window);

                // Stop the training radio and any sample still playing, so
                // nothing outlives the window (HM-DEC-027).
                (window.DataContext as MainWindowViewModel)?.ShutDownTraining();
            };

            desktop.MainWindow = window;
            desktop.ShutdownRequested += (_, _) =>
            {
                AppEvents.AppStop(_telemetry);
                _telemetry?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>Restore last size and position, clamped to a screen that
    /// still exists — a window restored onto a disconnected monitor is
    /// invisible, which reads as a crash.</summary>
    private void RestoreWindowState(Window window)
    {
        if (_settings.WindowWidth is > 400 && _settings.WindowHeight is > 300)
        {
            window.Width = _settings.WindowWidth.Value;
            window.Height = _settings.WindowHeight.Value;
        }

        if (_settings.WindowX is { } x && _settings.WindowY is { } y)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Position = new PixelPoint((int)x, (int)y);
            window.Opened += (_, _) => ClampToVisibleScreen(window);
        }

        if (_settings.WindowMaximized)
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    private static void ClampToVisibleScreen(Window window)
    {
        try
        {
            var screens = window.Screens;
            if (screens.ScreenFromWindow(window) is null)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                var primary = screens.Primary ?? screens.All[0];
                window.Position = primary.WorkingArea.TopLeft;
            }
        }
        catch (Exception)
        {
            // Screen enumeration is best-effort.
        }
    }

    private void SaveWindowState(Window window)
    {
        _settings.WindowMaximized = window.WindowState == WindowState.Maximized;
        if (window.WindowState == WindowState.Normal)
        {
            _settings.WindowX = window.Position.X;
            _settings.WindowY = window.Position.Y;
            _settings.WindowWidth = window.Width;
            _settings.WindowHeight = window.Height;
        }

        SettingsStore.Save(_settings);
        AppEvents.AppStop(_telemetry);
        _telemetry?.Dispose();
    }
}
