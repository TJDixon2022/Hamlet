using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 241, task 3: the decoded panel scrolls itself.
/// </summary>
/// <remarks>
/// <para>**THERE WAS NO SCROLLER AT ALL, AND A COMMENT SAYING THERE WAS.**
/// `MaxDigitalDecodes`'s own remark described "a plain `ItemsControl` inside a
/// `ScrollViewer`"; the markup had the items control in a `StackPanel`, so the
/// window's scroller carried the whole list. At fourteen rows a slot the
/// waterfall above it was pushed off the screen inside a minute, and only the
/// first few rows were reachable.</para>
/// <para>**THE HEIGHT IS THE ASSERTION, NOT THE SCROLLBAR.** A scrollbar that
/// appears while the panel still grows without bound would look like the fix and
/// not be one, so what is checked is that the list stops growing.</para>
/// </remarks>
public sealed class TheDecodedPanelScrollsItselfTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the heights are printed.</param>
    public TheDecodedPanelScrollsItselfTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>The row list is bounded however many rows arrive.</summary>
    [AvaloniaFact]
    public void ManyRowsDoNotMakeThePanelGrowWithoutBound()
    {
        var model = Model();
        var window = new MainWindow { DataContext = model };

        window.Show();
        Settle(window);

        Decode(model);
        Settle(window);

        var scroller = Scroller(window);

        Assert.NotNull(scroller);

        var withFew = scroller.Bounds.Height;
        var rowsWithFew = model.DigitalDecodes.Count;

        // Another two hundred rows, far past anything one slot delivers.
        for (var i = 0; i < 200; i++)
        {
            model.DigitalDecodes.Add(new DigitalDecodeRow(
                "214150", DigitalDecodeRow.NoMeasurement, "0.2",
                (600 + i).ToString(System.Globalization.CultureInfo.InvariantCulture),
                "CQ TA3MPK KM39"));
        }

        Settle(window);

        var withMany = scroller.Bounds.Height;

        _output.WriteLine("rows after one slot : " + rowsWithFew
            + ", list height " + withFew.ToString("0.#"));
        _output.WriteLine("rows after 200 more : " + model.DigitalDecodes.Count
            + ", list height " + withMany.ToString("0.#"));
        _output.WriteLine("the cap on the list : 300");

        Assert.True(
            withMany <= 300.5,
            "the row list grew to " + withMany.ToString("0.#")
            + " px, so it is still pushing the rest of the tab off the screen");

        // And the extent really is larger than the viewport, so there is
        // something to scroll rather than rows quietly not being laid out.
        Assert.True(
            scroller.Extent.Height > scroller.Viewport.Height,
            "the content is not taller than the viewport, so nothing was "
            + "actually scrollable and this proves nothing");
    }

    /// <summary>
    /// A reader who has scrolled away is left where they put themselves.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE HALF THAT MAKES THE PANEL USABLE.** Rows arrive fourteen at
    /// a time every fifteen seconds. A list that jumps back to the live end on
    /// every batch would make reading a callsign somebody missed impossible.
    /// </remarks>
    [AvaloniaFact]
    public void ScrollingAwayIsRespectedAndTheLiveEndIsFollowed()
    {
        var model = Model();
        var window = new MainWindow { DataContext = model };

        window.Show();
        Settle(window);

        Decode(model);

        for (var i = 0; i < 60; i++)
        {
            model.DigitalDecodes.Add(new DigitalDecodeRow(
                "214150", DigitalDecodeRow.NoMeasurement, "0.2",
                (600 + i).ToString(System.Globalization.CultureInfo.InvariantCulture),
                "CQ TA3MPK KM39"));
        }

        Settle(window);

        var scroller = Scroller(window);

        Assert.NotNull(scroller);

        // Park the reader in the middle, well away from either end.
        var middle = (scroller.Extent.Height - scroller.Viewport.Height) / 2;

        scroller.Offset = scroller.Offset.WithY(middle);
        Settle(window);

        var parked = scroller.Offset.Y;

        model.DigitalDecodes.Add(new DigitalDecodeRow(
            "214205", DigitalDecodeRow.NoMeasurement, "0.3", "1500",
            "W4WTM K1ABC RR73"));

        Settle(window);

        _output.WriteLine("parked at   : " + parked.ToString("0.#"));
        _output.WriteLine("after a row : " + scroller.Offset.Y.ToString("0.#"));

        Assert.True(
            Math.Abs(scroller.Offset.Y - parked) < 1,
            "the list moved from " + parked.ToString("0.#") + " to "
            + scroller.Offset.Y.ToString("0.#")
            + " while somebody was reading it");

        // --- and at the live end it follows ---------------------------------
        scroller.ScrollToEnd();
        Settle(window);

        model.DigitalDecodes.Add(new DigitalDecodeRow(
            "214220", DigitalDecodeRow.NoMeasurement, "0.1", "1800",
            "TA3MPK W4WTM 73"));

        Settle(window);

        var fromEnd = scroller.Extent.Height
            - scroller.Offset.Y
            - scroller.Viewport.Height;

        _output.WriteLine("at the end, after a row, distance from end: "
            + fromEnd.ToString("0.#"));

        Assert.True(
            fromEnd < 40,
            "the list did not follow the new row: it is " + fromEnd.ToString("0.#")
            + " px from the end");
    }

    private static ScrollViewer? Scroller(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalDecodedRows")
            ?.FindAncestorOfType<ScrollViewer>();

    private static void Settle(MainWindow window)
    {
        for (var i = 0; i < 8; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static MainWindowViewModel Model()
        => new(new AppSettings(), null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

    /// <summary>One real decode, so `HasDigitalDecodes` is raised properly.</summary>
    private static void Decode(MainWindowViewModel model)
    {
        var samples = new float[Rate * 30];
        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "TA3MPK", "KM39", message));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(message), Rate, 1240f);

        slot.CopyTo(samples.AsSpan(13 * Rate));

        model.ShowDecodes(
            new MonoAudio(Rate, samples),
            new DateTime(2026, 9, 4, 21, 41, 47, DateTimeKind.Utc),
            new ClockOffset(0, new DateTime(2026, 9, 4, 21, 40, 0, DateTimeKind.Utc)));
    }
}
