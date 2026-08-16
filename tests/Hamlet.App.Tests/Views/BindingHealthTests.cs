using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(Hamlet.App.Tests.Views.HeadlessApp))]

namespace Hamlet.App.Tests.Views;

/// <summary>Builds the real application, without a screen.</summary>
public static class HeadlessApp
{
    /// <summary>The app under test.</summary>
    /// <returns>A configured builder.</returns>
    public static Avalonia.AppBuilder BuildAvaloniaApp()
        => Avalonia.AppBuilder.Configure<Hamlet.App.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

/// <summary>Collects everything Avalonia complains about while a window runs.</summary>
internal sealed class Complaints : ILogSink
{
    public List<string> Lines { get; } = new();

    public bool IsEnabled(LogEventLevel level, string area)
        => level >= LogEventLevel.Warning;

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
        => Add(level, area, messageTemplate, Array.Empty<object?>());

    public void Log(
        LogEventLevel level, string area, object? source,
        string messageTemplate, params object?[] propertyValues)
        => Add(level, area, messageTemplate, propertyValues);

    private void Add(
        LogEventLevel level, string area, string template, object?[] values)
    {
        if (level < LogEventLevel.Warning)
        {
            return;
        }

        var text = template;

        for (var i = 0; i < values.Length; i++)
        {
            text = Replace(text, values[i]);
        }

        Lines.Add($"[{area}] {text}");
    }

    private static string Replace(string text, object? value)
    {
        var open = text.IndexOf('{');
        var close = open < 0 ? -1 : text.IndexOf('}', open);

        return close < 0 ? text : text[..open] + value + text[(close + 1)..];
    }
}

/// <summary>
/// **THE WINDOW IS BUILT AND EVERY BINDING IN IT HAS TO RESOLVE**
/// (HM-DEC-087).
/// </summary>
/// <remarks>
/// <para>**THIS IS THE TEST THAT WOULD HAVE CAUGHT ALL THREE OF THEM.** A
/// binding whose path does not resolve does not throw and does not fail the
/// build. Avalonia writes a line to a log nobody was reading and carries on with
/// null, and a button whose Command is null renders and behaves exactly like a
/// disabled one. Seventeen controls on the canvas shipped dead that way, and
/// they sat beside live controls that looked identical.</para>
/// <para>So the real window is built against the real view model, headless, and
/// anything Avalonia would have written to that log fails the test instead. It
/// costs a second and it closes the whole class of fault rather than the
/// instances of it that somebody happened to notice (§0: where a check can run
/// in CI, run it in CI).</para>
/// </remarks>
public sealed class BindingHealthTests
{
    /// <remarks>
    /// Proves HM-DEC-087. Every binding in the main window resolves, including
    /// the ones behind the canvas commands, which resolved to null from first
    /// paint and made nine tray items, three preset buttons, four widget close
    /// buttons and the bring-it-back button dead on arrival.
    /// </remarks>
    [AvaloniaFact]
    public void TheMainWindowBindsWithoutOneComplaint()
    {
        var complaints = new Complaints();
        var was = Logger.Sink;
        Logger.Sink = complaints;

        // THE OPERATOR'S OWN ARRANGEMENT IS NOT THIS TEST'S TO READ OR WRITE
        // (HM-DEC-089). The real view model loads and saves the canvas, so
        // without this the test both depends on and can overwrite whatever is
        // on the machine it happens to run on.
        var layouts = Hamlet.App.Layout.LayoutStore.Path;
        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(new AppSettings(), null),
            };

            window.Show();

            // Enough turns of the loop for the item templates to be realized,
            // which is when the container-relative bindings are evaluated.
            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            window.Close();
        }
        finally
        {
            Logger.Sink = was;

            try
            {
                File.Delete(Hamlet.App.Layout.LayoutStore.Path);
            }
            catch (IOException)
            {
                // A leftover temporary file is not a failing test.
            }

            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }

        var bindings = complaints.Lines
            .Where(l => l.Contains("[Binding]", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            bindings.Count == 0,
            "the window has bindings that do not resolve, and a control bound to "
            + "nothing looks and behaves exactly like a disabled one:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, bindings));
    }
}
