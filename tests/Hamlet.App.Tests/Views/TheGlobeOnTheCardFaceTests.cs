using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
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
/// Work instruction 308 task 4: **the map moves onto the card face, where its markers
/// can own their own hovers.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE AUTHOR'S PROPOSAL AND NOT TIM'S RULING**, reproduced in
/// `output.md` for him to overrule. Unit 306 built both sets of marker words, asserted
/// them, and could not attach them, because the map lived inside a tooltip and a
/// tooltip inside a tooltip is not a thing. **A picture that has to be hovered to be
/// seen is a picture he will not see.**</para>
/// <para>**A CONTROL FOUND IN THE WINDOW'S VISUAL TREE IS ON THE FACE.** A tooltip's
/// contents are not in the visual tree until the tooltip is shown, so finding the map
/// by walking the window from its root is the assertion - it is computed, like every
/// appearance claim in this repository, and says nothing about what it looks
/// like.</para>
/// </remarks>
public sealed class TheGlobeOnTheCardFaceTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public TheGlobeOnTheCardFaceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The map is on the card face, not inside a tooltip.**</summary>
    [AvaloniaFact]
    public void TheMapIsOnTheCardFaceAndNotInsideATooltip()
    {
        var window = Standing(out var model);

        var maps = window.GetVisualDescendants()
            .OfType<Ft8GlobeControl>()
            .ToList();

        _output.WriteLine("cards on the panel : " + model.DigitalCards.Count);
        _output.WriteLine("maps in the window : " + maps.Count);

        Assert.NotEmpty(model.DigitalCards);

        // **IT IS IN THE TREE**, which a tooltip's contents are not until shown.
        Assert.NotEmpty(maps);

        // **AND IT IS INSIDE THE CARD**, rather than somewhere else on the screen.
        Assert.Contains(
            maps[0].GetVisualAncestors().OfType<Control>(),
            c => c.Classes.Contains("hm-card"));
    }

    /// <summary>**The `i` keeps its rows and loses the map.**</summary>
    [AvaloniaFact]
    public void TheHintKeepsItsRowsAndLosesTheMap()
    {
        Standing(out var model);

        var card = model.DigitalCards.Single();

        foreach (var row in card.DetailRows)
        {
            _output.WriteLine(
                row.Length.ToString(CultureInfo.InvariantCulture).PadLeft(3)
                + "  " + row);
        }

        Assert.Equal(5, card.DetailRows.Count);

        Assert.All(
            card.DetailRows,
            row => Assert.True(
                row.Length <= 66,
                "a row is " + row.Length + " characters: " + row));
    }

    /// <summary>**Each marker carries its own words.**</summary>
    [AvaloniaFact]
    public void EachMarkerCarriesItsOwnWords()
    {
        Standing(out var model);

        var globe = model.DigitalCards.Single().Globe;

        _output.WriteLine("operator: " + globe.OperatorTip);
        _output.WriteLine("station : " + globe.StationTip);

        Assert.Contains("You", globe.OperatorTip, StringComparison.Ordinal);
        Assert.Contains(Station, globe.StationTip, StringComparison.Ordinal);
        Assert.Contains("miles", globe.StationTip, StringComparison.Ordinal);

        // **A COMPASS WORD, NOT DEGREES** - unit 306's amendment, carried forward.
        Assert.DoesNotContain("degrees", globe.StationTip, StringComparison.Ordinal);

        // **AND THE CONTROL KNOWS WHICH MARKER THE POINTER IS ON.**
        var map = new Ft8GlobeControl { Plot = globe };

        Assert.Equal(
            globe.StationTip,
            map.WordsAt(globe.StationX, globe.StationY, Ft8GlobePlot.Map.WidthPixels));

        Assert.Equal(
            globe.OperatorTip,
            map.WordsAt(globe.OperatorX, globe.OperatorY, Ft8GlobePlot.Map.WidthPixels));

        // **AND SAYS NOTHING WHERE THERE IS NO MARKER.**
        Assert.Null(map.WordsAt(5, 5, Ft8GlobePlot.Map.WidthPixels));
    }

    /// <summary>**A station with no place has no marker and so no words.**</summary>
    [AvaloniaFact]
    public void AStationWithNoPlaceHasNoMarkerAndNoWords()
    {
        // **ANTARCTICA**, which is one of the two places this picture cannot show.
        var globe = new Ft8GlobePlot("FN00DJ", "RB32", "KC4AAA", "Antarctica");

        _output.WriteLine("caption: " + globe.Caption);
        _output.WriteLine("station words: [" + globe.StationTip + "]");

        Assert.False(globe.HasStation);
        Assert.Empty(globe.StationTip);

        var map = new Ft8GlobeControl { Plot = globe };

        Assert.Null(map.WordsAt(globe.StationX, globe.StationY, Ft8GlobePlot.Map.WidthPixels));

        // **AND THE CAPTION SAYS WHICH OF THE TWO REASONS IT IS.**
        Assert.Contains("nowhere on it", globe.Caption, StringComparison.Ordinal);

        Assert.DoesNotContain(
            "does not know where", globe.Caption, StringComparison.Ordinal);

        // **THE OTHER REASON STILL READS AS ITSELF.**
        var noGrid = new Ft8GlobePlot("FN00DJ", null, "K9XP", "");

        _output.WriteLine("no grid: " + noGrid.Caption);

        Assert.Contains(
            "does not know where", noGrid.Caption, StringComparison.Ordinal);
    }

    /// <summary>The real window with one station card on it.</summary>
    private static MainWindow Standing(out MainWindowViewModel model)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");
        Heard(model, "02:11:45", HisCall + " " + Station + " -09");
        Heard(model, "02:12:00", HisCall + " " + Station + " RR73");

        model.CardsNowForTests = Slot("02:12:15");
        model.RebuildCardsForTests();

        var window = new MainWindow { DataContext = model };

        window.Show();

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        return window;
    }

    private static void Sent(MainWindowViewModel model, string at, string message)
        => model.RecordSentForTests(message, Slot(at));

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
