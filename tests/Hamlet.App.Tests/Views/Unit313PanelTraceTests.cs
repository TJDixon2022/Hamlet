using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 313 task 1d: **what the For You panel does with more cards than
/// fit.**
/// </summary>
/// <remarks>
/// <para>**IT STANDS THE REAL WINDOW UP.** The same headless window
/// `BindingHealthTests` builds, with six conversations put through the real decode path,
/// so what is read back is the layout the application actually performs rather than a
/// description of the markup.</para>
/// <para>**COMPUTED, NOT SEEN.** A headless window lays out and does not paint. What is
/// asserted is where the layout put each card and what the container is; nothing here
/// can say whether a scrollbar is visible to a person.</para>
/// </remarks>
public sealed class Unit313PanelTraceTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the layout is printed.</param>
    public Unit313PanelTraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Six cards, and what the panel does with the sixth.**</summary>
    [AvaloniaFact]
    public void SixCardsAndWhatThePanelDoesWithTheSixth()
    {
        var model = Panel();

        var callers = new[] { "K9XP", "W1ABC", "VE3XN", "G0ABC", "JA1ABC", "VK2ABC" };

        for (var i = 0; i < callers.Length; i++)
        {
            Heard(model, "02:1" + i + ":15", HisCall + " " + callers[i] + " EN52");
        }

        model.CardsNowForTests = Slot("02:16:00");
        model.RebuildCardsForTests();

        Assert.Equal(callers.Length, model.DigitalCards.Count);

        var window = new MainWindow { DataContext = model, Width = 1400, Height = 900 };

        window.Show();

        for (var i = 0; i < 10; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        var cards = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalContactCards");

        Assert.NotNull(cards);

        _output.WriteLine("the cards control is "
            + cards!.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture) + " by "
            + cards.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture));

        // **IS THERE A SCROLL CONTAINER ANYWHERE ABOVE IT?**
        var scroll = cards.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();

        _output.WriteLine("nearest ScrollViewer above it: "
            + (scroll is null
                ? "NONE"
                : scroll.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                  + " by "
                  + scroll.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)));

        // **WHAT CONTAINS IT, UP TO THE PANEL.**
        foreach (var up in cards.GetVisualAncestors().Take(6))
        {
            _output.WriteLine("  in " + up.GetType().Name
                + " " + ((Visual)up).Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " by " + ((Visual)up).Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture));
        }

        // **WHERE EACH CARD LANDED**, and whether the last one is inside the panel.
        var panel = cards.GetVisualAncestors()
            .OfType<Control>()
            .FirstOrDefault(c => c.Name == "DigitalMinePanel");

        Assert.NotNull(panel);

        _output.WriteLine("the panel is "
            + panel!.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture) + " by "
            + panel.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture));

        var realized = cards.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .Where(p => p.DataContext is Ft8ContactCard)
            .ToList();

        _output.WriteLine("realized card containers: " + realized.Count);

        foreach (var one in realized)
        {
            var card = (Ft8ContactCard)one.DataContext!;

            var top = one.TranslatePoint(new Point(0, 0), panel);

            _output.WriteLine("  " + card.Callsign.PadRight(8)
                + " at y " + (top?.Y.ToString("0.0", CultureInfo.InvariantCulture) ?? "?")
                + ", height " + one.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)
                + ", width " + one.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + (top is { } p && p.Y + one.Bounds.Height > panel.Bounds.Height
                    ? "   <- PAST THE BOTTOM OF THE PANEL"
                    : ""));
        }

        window.Close();
    }

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ClockOffset = new Hamlet.RadioEngine.Audio.ClockOffset(
            0.033, DateTime.UtcNow);

        return model;
    }

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
