using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 281, task 4: the contact count wears a belt.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: a coloured ring that rises with the count,
/// martial-arts order, **and gold at ten thousand — go for the gold.**</para>
/// <para>**AND THE RANK IS A FLOURISH ON TOP OF A NUMBER** (§0.6). The load-bearing
/// test here is the grayscale one: the count and the progress must still read with
/// every hue stripped out, and only the rank may be lost. A belt that was the only
/// way to know anything would be colour doing work nothing else does.</para>
/// </remarks>
public sealed class TheCountWearsARankTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the ten ranks are printed.</param>
    public TheCountWearsARankTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Each of the ten counts yields its own rank, in order.**</summary>
    /// <remarks>
    /// Watched failing first: before `ContactBelt` existed this did not compile, and
    /// with only the nine old thresholds a count of 0 had no rank at all.
    /// </remarks>
    [AvaloniaFact]
    public void EachOfTheTenCountsYieldsItsOwnRank()
    {
        var expected = new (int Count, string Rank)[]
        {
            (0, "white"), (10, "yellow"), (25, "orange"), (50, "green"),
            (100, "blue"), (500, "purple"), (1000, "brown"), (2000, "red"),
            (5000, "black"), (10000, "gold"),
        };

        foreach (var (count, rank) in expected)
        {
            var got = ContactBelt.For(count);

            _output.WriteLine(
                count.ToString("N0", CultureInfo.InvariantCulture).PadLeft(7)
                + "  " + got.Name.PadRight(7) + got.Ink);

            Assert.Equal(rank, got.Name);
            Assert.Equal(count, got.At);
        }

        // **TEN RANKS AND TEN INKS.** Two ranks sharing a colour would make the
        // ring say less than it looks like it is saying.
        Assert.Equal(10, ContactBelt.Ranks.Count);
        Assert.Equal(10, ContactBelt.Ranks.Select(r => r.Ink).Distinct().Count());

        // **ONE LIST, ONE PLACE.** The badge thresholds are the belt without white.
        Assert.Equal(
            ContactBelt.Ranks.Where(r => r.At > 0).Select(r => r.At).ToList(),
            ContactMilestones.Thresholds);
    }

    /// <summary>**A count between two ranks keeps the lower one.**</summary>
    [AvaloniaFact]
    public void ACountBetweenTwoRanksKeepsTheLowerOne()
    {
        foreach (var (count, rank) in new[]
        {
            (1, "white"), (9, "white"), (24, "yellow"), (99, "green"),
            (499, "blue"), (9_999, "black"), (40_000, "gold"),
        })
        {
            _output.WriteLine(count.ToString("N0", CultureInfo.InvariantCulture)
                + " -> " + ContactBelt.For(count).Name);

            Assert.Equal(rank, ContactBelt.For(count).Name);
        }
    }

    /// <summary>**The ring is a border and it is never filled.**</summary>
    /// <remarks>
    /// HM-DEC-012 and §0.5. A filled disc in a status bar reads as a status light,
    /// which is a claim about whether something is working rather than about how
    /// many contacts he has made.
    /// </remarks>
    [AvaloniaFact]
    public void TheRingIsABorderAndNotAFill()
    {
        var window = Screen(120);

        var ring = Named<Border>(window, "ContactBeltRing");

        _output.WriteLine("border : " + Describe(ring.BorderBrush));
        _output.WriteLine("fill   : " + Describe(ring.Background));
        _output.WriteLine("width  : " + ring.BorderThickness);

        Assert.True(ring.BorderThickness.Top > 0, "the ring has no border at all");

        // **TRANSPARENT IS THE ONLY ACCEPTABLE BACKGROUND** — it is there so the
        // whole ring is one hover target, not so it carries a colour.
        var fill = (ring.Background as ISolidColorBrush)?.Color;

        Assert.True(
            fill is null || fill.Value.A == 0,
            "the ring is filled with " + Describe(ring.Background));

        // And the border is this rank's own ink: 120 contacts is the blue belt.
        Assert.Equal("blue", ContactBelt.For(120).Name);
        Assert.Equal(
            Color.Parse(ContactBelt.For(120).Ink),
            (ring.BorderBrush as ISolidColorBrush)?.Color);
    }

    /// <summary>**In grayscale the number and the bar still read.**</summary>
    /// <remarks>
    /// <para>§0.6's own practical test, applied to this widget. Every hue is thrown
    /// away and what is left has to answer two questions: how many contacts, and how
    /// far to the next rank. Only which colour the ring is may be lost.</para>
    /// <para>Watched failing first: with the bar drawn in the belt's own ink, two
    /// different ranks at the same fraction were indistinguishable from each other
    /// once the hue was gone, and the bar was carrying the rank as well.</para>
    /// </remarks>
    [AvaloniaFact]
    public void InGrayscaleTheNumberAndTheBarStillRead()
    {
        foreach (var count in new[] { 6, 120, 10_000 })
        {
            var window = Screen(count);

            var number = Named<TextBlock>(window, "ContactCountNumber");
            var bar = Named<BadgeProgressControl>(window, "ContactBadgeProgress");

            _output.WriteLine(
                count.ToString("N0", CultureInfo.InvariantCulture).PadLeft(7)
                + "  number \"" + number.Text + "\"  bar "
                + (bar.IsVisible ? bar.Fraction.ToString("0.00") : "not drawn")
                + "  rank " + ContactBelt.For(count).Name);

            // **THE NUMBER IS THE PRIMARY CARRIER AND IT IS NOT A COLOUR.**
            Assert.Equal(
                count.ToString("N0", CultureInfo.InvariantCulture), number.Text);

            Assert.True(number.FontSize >= 18, "the count is not the big thing");

            // **THE PROGRESS IS A LENGTH, WHICH SURVIVES ANY PRINTER.** At the top
            // of the belt there is nowhere further to go and it is not drawn.
            if (ContactBelt.After(count) is null)
            {
                Assert.False(bar.IsVisible);
                continue;
            }

            Assert.True(bar.IsVisible);
            Assert.InRange(bar.Fraction, 0.0, 1.0);
        }

        // Six of the ten to the yellow belt is six tenths of the way there, and
        // that is a measurement rather than a decoration (§0.0).
        Assert.Equal(0.6, ContactBelt.Progress(6), 3);
        Assert.Equal(0.0, ContactBelt.Progress(0), 3);
        Assert.Equal(1.0, ContactBelt.Progress(10_000), 3);
    }

    /// <summary>**The words are on the hover: the rank, the next, and how far.**</summary>
    [AvaloniaFact]
    public void HoveringTheRingNamesTheRankAndWhatIsNext()
    {
        var window = Screen(4);

        var ring = Named<Border>(window, "ContactBeltRing");
        var tip = ToolTip.GetTip(ring) as string ?? "";

        _output.WriteLine(tip);

        Assert.Contains("white belt", tip, StringComparison.Ordinal);
        Assert.Contains("6 more contacts", tip, StringComparison.Ordinal);
        Assert.Contains("yellow", tip, StringComparison.Ordinal);
        Assert.Contains("10", tip, StringComparison.Ordinal);

        // One more reads as one more, not as "1 more contacts".
        Assert.Contains("One more contact", ContactBelt.Tip(9), StringComparison.Ordinal);

        // And the top of the belt says so rather than promising an eleventh rank.
        _output.WriteLine(ContactBelt.Tip(12_000));
        Assert.Contains("top of the belt", ContactBelt.Tip(12_000), StringComparison.Ordinal);
    }

    /// <summary>The main window with a log of a given size behind it.</summary>
    /// <param name="count">How many contacts the panel should believe are logged.</param>
    /// <returns>The realized window.</returns>
    private static Window Screen(int count)
    {
        // **A REAL LOG FILE, NOT A SEAM** (§12.5). The count is read off the ADIF
        // the operator's own log lives in, and a test-only setter would have proved
        // the ring against a number nothing else in the application produces.
        WriteLog(count);

        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        return window;
    }

    /// <summary>Put a log of a given size where the panel will read it.</summary>
    /// <param name="count">How many records to write.</param>
    /// <remarks>
    /// **THROUGH THE WRITER** (§12.5). A fixture built from the same understanding
    /// as the code proves nothing, so the records go through `AdifLog` rather than
    /// being assembled by hand here.
    /// </remarks>
    private static void WriteLog(int count)
    {
        var entries = Enumerable.Range(0, count).Select(i => AdifLog.Record(
            new AdifContact
            {
                Call = "T" + i.ToString(CultureInfo.InvariantCulture),
                StationCallsign = "KC3QIS",
                StartedUtc = new DateTime(2026, 9, 8, 2, 13, 15, DateTimeKind.Utc),
                Mode = "FT8",
            }));

        System.IO.File.WriteAllText(
            ContactLogStore.LogPath,
            AdifLog.Header("1.12.176") + string.Concat(entries));
    }

    /// <summary>One named control on the realized window.</summary>
    /// <typeparam name="T">What it should be.</typeparam>
    /// <param name="window">Where to look.</param>
    /// <param name="name">Its name in the markup.</param>
    /// <returns>The control.</returns>
    private static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "no " + typeof(T).Name + " named " + name + " on the realized window. "
            + typeof(T).Name + "s found: ["
            + string.Join(", ", window.GetVisualDescendants().OfType<T>()
                .Where(c => c.Name is not null).Select(c => c.Name)) + "]");

        return found!;
    }

    /// <summary>A brush, for the printout.</summary>
    /// <param name="brush">The brush, or null.</param>
    /// <returns>Something readable.</returns>
    private static string Describe(IBrush? brush)
        => brush is ISolidColorBrush solid ? solid.Color.ToString() : "none";
}
