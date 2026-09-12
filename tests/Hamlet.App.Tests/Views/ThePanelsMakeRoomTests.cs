using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
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
/// Work instruction 331 task 1a: **the panels make room.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The decoded text doesn't have to be as wide as it is. Move
/// the clear button up above both decoded and for you, squeeze the decoded text in, and
/// make the for you a little wider."*</para>
/// <para>**WHY IT IS A RULING AND NOT A PREFERENCE.** Unit 330 built the conversation
/// card's facts as a table and then measured that the table did not fit beside the map: on
/// a 1400 px window the *For you* panel was a quarter of the tab, the card was 275 px wide
/// inside, the map wants 222 of them and the widest row wants about 190. So 330 put the
/// table under the picture and asked. This is the answer - the room comes from the
/// decoded list, which does not need the width it has.</para>
/// <para>**COMPUTED, NOT SEEN.** Every number below is read off a realized headless window
/// at 1400 x 1200, which is the shape Tim's screenshots come from, and printed so the
/// arithmetic can be checked.</para>
/// </remarks>
public sealed class ThePanelsMakeRoomTests
{
    /// <summary>His call, and a station far enough away for a long distance row.</summary>
    private const string HisCall = "KC3QIS";

    private const string Station = "K9XP";

    /// <summary>Vienna - a long path and a compass word, so the table's widest row renders.</summary>
    private const string FarGrid = "JN88";

    /// <summary>
    /// **The longest line the decoded list has to hold, and it is a fixture and not a
    /// guess.** Twenty-two characters: two six-character callsigns, a signed report with a
    /// roger, and the spaces between them. Nothing standard in FT8 is longer, and the
    /// column is sized to it rather than to the average.
    /// </summary>
    private const string LongestLine = "VP2MAA/P KC3QIS R-09";

    /// <summary>**What the card needs inside it**, from work instruction 331 task 1a.</summary>
    /// <remarks>
    /// 222 px of map, 190 px for the table's widest row, and the margins between and
    /// either side. The instruction states 460 and that is what is asserted.
    /// </remarks>
    private const double CardWantsInside = 460;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public ThePanelsMakeRoomTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: one bar above both panels carries the three controls.**
    /// </summary>
    /// <remarks>
    /// <para>The filter - `everything` and `CQ` - and the two row controls - the order
    /// toggle and `clear` - used to live in the *Decoded text* panel's own header, which is
    /// where §0.5 puts a panel's controls. They govern a list that is now the narrow half,
    /// and Tim asked for them above both panels.</para>
    /// <para>**ABOVE, AND SPANNING.** The bar's bottom edge is at or above the top of each
    /// panel, and its width covers both of them - so it is one bar over the pair rather
    /// than a bar over one of them.</para>
    /// </remarks>
    [AvaloniaFact]
    public void OneBarAboveBothPanelsCarriesTheThreeControls()
    {
        var window = Realized(1400);

        var bar = window.FindControl<Control>("DigitalListControlsBar");

        Assert.True(bar is not null, "there is no control named DigitalListControlsBar");

        var decoded = window.FindControl<Control>("DigitalDecodedPanel")!;
        var mine = window.FindControl<Control>("DigitalMinePanel")!;
        var panes = window.FindControl<Control>("DigitalDecodedPanes")!;

        _output.WriteLine("bar    : " + Box(bar!));
        _output.WriteLine("decoded: " + Box(decoded));
        _output.WriteLine("for you: " + Box(mine));
        _output.WriteLine("panes  : " + Box(panes));

        // **THE THREE CONTROLS ARE ON THE BAR AND NOWHERE ELSE.**
        foreach (var name in new[]
        {
            "DigitalFilterEverything", "DigitalFilterCq",
            "DigitalOrderToggle", "DigitalClearPress",
        })
        {
            var control = window.FindControl<Control>(name);

            Assert.True(control is not null, name + " is not in the window at all");

            Assert.True(
                bar!.GetVisualDescendants().Contains(control!),
                name + " is not on the bar above the panels");

            Assert.False(
                decoded.GetVisualDescendants().Contains(control!),
                name + " is still inside the Decoded text panel");

            Assert.False(
                mine.GetVisualDescendants().Contains(control!),
                name + " is inside the For you panel");
        }

        // **ABOVE BOTH.** A bar drawn over one panel and beside the other is the thing
        // this replaces.
        var barBottom = bar!.Bounds.Bottom + Offset(bar, panes);
        var decodedTop = decoded.Bounds.Top + Offset(decoded, panes);
        var mineTop = mine.Bounds.Top + Offset(mine, panes);

        _output.WriteLine("");
        _output.WriteLine(
            "bar bottom " + Px(barBottom) + " against decoded top " + Px(decodedTop)
            + " and for you top " + Px(mineTop) + ", all in the panes' own frame");

        Assert.True(barBottom <= decodedTop + 0.51, "the bar is not above the decoded list");
        Assert.True(barBottom <= mineTop + 0.51, "the bar is not above the For you panel");

        // **AND SPANNING THEM.** The bar's own row spans both columns, so the room it has
        // is the pair's width and not one panel's.
        Assert.True(
            bar.Bounds.Width + 0.51 >= decoded.Bounds.Width + mine.Bounds.Width - 12,
            "the bar is " + Px(bar.Bounds.Width) + " wide against "
            + Px(decoded.Bounds.Width + mine.Bounds.Width) + " of panels, so it does not span them");
    }

    /// <summary>
    /// **Assertion 2: the decoded list's message column fits the longest fixture line.**
    /// </summary>
    /// <remarks>
    /// **THIS SIDE CLIPS RATHER THAN WRAPPING** (unit 273), so a message column narrower
    /// than its content is a callsign cut off partway, which is a station misidentified and
    /// §0.0 broken. The list is being squeezed, so the guard is the point of the squeeze:
    /// the cell gets at least what it asks for.
    /// </remarks>
    [AvaloniaFact]
    public void TheDecodedListsMessageColumnFitsTheLongestFixtureLine()
    {
        var window = Realized(1400);

        var cells = window.FindControl<ItemsControl>("DigitalDecodedRows")!
            .GetVisualDescendants().OfType<StackPanel>()
            .Where(p => p.Name == "DecodedMessageCell" && p.IsVisible)
            .ToList();

        Assert.NotEmpty(cells);

        var decoded = window.FindControl<Control>("DigitalDecodedPanel")!;

        _output.WriteLine("decoded panel: " + Px(decoded.Bounds.Width) + " px wide");
        _output.WriteLine("");

        foreach (var cell in cells)
        {
            _output.WriteLine(
                "message cell: " + Px(cell.Bounds.Width) + " px, wants "
                + Px(cell.DesiredSize.Width) + " px - "
                + string.Concat(cell.GetVisualDescendants().OfType<TextBlock>()
                    .Select(t => t.Text)));
        }

        // **`DesiredSize` IS NOT THE ANSWER AND THAT MATTERS.** A `StackPanel` measured
        // inside 195 px reports that it desires 195, so a cell being clipped by five
        // pixels reports a perfect fit. The need is measured from the text itself,
        // unconstrained, in the row's own typeface.
        var need = Measure(LongestLine);

        _output.WriteLine("");
        _output.WriteLine(
            "the longest FT8 line, " + LongestLine + ", is " + LongestLine.Length
            + " characters and measures " + Px(need) + " px unconstrained in the row's"
            + " own monospace at 12, which is " + Px(need / LongestLine.Length)
            + " px a character on this host");

        var widest = cells.Max(c => c.Bounds.Width);

        _output.WriteLine(
            "the message column is " + Px(widest) + " px, so the headroom is "
            + Px(widest - need) + " px");

        Assert.True(
            widest + 0.51 >= need,
            "the message column is " + Px(widest) + " px and the longest FT8 line needs "
            + Px(need) + " px, so a callsign is cut off partway");
    }

    /// <summary>
    /// **Assertion 3: For you takes what is left, and on a wide window that is at least
    /// the 460 px inside the card that the map and the table need.**
    /// </summary>
    /// <remarks>
    /// <para>**THAT IS THE NUMBER TASK 1a NAMES**: 222 px of map, 190 px for the table's
    /// widest row, and the margins between and either side. Unit 330 measured 275 and
    /// could not fit them, so this is the assertion that says the room now exists.</para>
    /// <para>**AND IT IS ASSERTED AT 1920 AND ONLY REPORTED AT 1400, BECAUSE THE
    /// ARITHMETIC DOES NOT REACH 460 ON A 1400 px WINDOW AND SAYING OTHERWISE WOULD BE A
    /// FALSE CLAIM** (§0.0). The decoded panes are half the tab, the decoded list needs
    /// 383 of them to stop clipping, and 460 inside the card needs about 516 px of panel -
    /// so the pair needs about 899 px, which is a window of about 1856. Both numbers are
    /// printed; output.md reports the threshold.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ForYouTakesWhatIsLeftAndOnAWideWindowThatIsTheRoomTheTableNeeds()
    {
        foreach (var width in new[] { 1400.0, 1920.0 })
        {
            var window = Realized(width);

            var mine = window.FindControl<Control>("DigitalMinePanel")!;
            var decoded = window.FindControl<Control>("DigitalDecodedPanel")!;
            var panes = window.FindControl<Control>("DigitalDecodedPanes")!;
            var cards = window.FindControl<ItemsControl>("DigitalContactCards")!;

            // **`CardBeside` IS A `WrapPanel` SINCE 331 TASK 2**, so it is looked up as a
            // `Control` rather than as a `Grid`: the map and the table sit side by side
            // when the card is wide enough and the table takes its own line when it is
            // not, which is what stops a narrow card truncating it.
            var card = cards.GetVisualDescendants().OfType<Control>()
                .FirstOrDefault(c => c.Name == "CardBeside");

            _output.WriteLine("window " + Px(width) + " x 1200");
            _output.WriteLine("  panes      : " + Px(panes.Bounds.Width));
            _output.WriteLine(
                "  decoded    : " + Px(decoded.Bounds.Width) + " px, "
                + Px(100 * decoded.Bounds.Width / panes.Bounds.Width) + "% of the pair");
            _output.WriteLine(
                "  for you    : " + Px(mine.Bounds.Width) + " px, "
                + Px(100 * mine.Bounds.Width / panes.Bounds.Width) + "% of the pair");
            _output.WriteLine("  cards list : " + Px(cards.Bounds.Width));
            _output.WriteLine(
                "  card inside: "
                + (card is null ? "no CardBeside grid" : Px(card.Bounds.Width)));
            _output.WriteLine("");

            Assert.True(card is not null, "the card has no grid named CardBeside");

            // **FOR YOU IS THE WIDER OF THE TWO ON A WIDE WINDOW**, which is the half of
            // the ruling that holds at every width above the decoded list's own need.
            if (width >= 1600)
            {
                Assert.True(
                    mine.Bounds.Width > decoded.Bounds.Width,
                    "For you is " + Px(mine.Bounds.Width) + " px against the decoded "
                    + Px(decoded.Bounds.Width) + ", so it is not the wider panel");

                Assert.True(
                    card!.Bounds.Width + 0.51 >= CardWantsInside,
                    "the card is " + Px(card.Bounds.Width) + " px wide inside against the "
                    + Px(CardWantsInside) + " the map and the table need");
            }
        }
    }

    /// <summary>
    /// **Assertion 4: the decoded list is narrower than it was, and by a stated amount.**
    /// </summary>
    /// <remarks>
    /// **THE BEFORE IS `*,*`** - the two panels were the same width by construction, which
    /// is what `WhatTheSplitCostsTests` asserted until this unit. Half of the pair at any
    /// width is the figure the split is compared against here, so the test carries the
    /// *before* rather than leaving it in a commit message.
    /// </remarks>
    [AvaloniaFact]
    public void TheDecodedListGivesUpWhatItDoesNotNeed()
    {
        var window = Realized(1920);

        var decoded = window.FindControl<Control>("DigitalDecodedPanel")!;
        var mine = window.FindControl<Control>("DigitalMinePanel")!;
        var panes = window.FindControl<Control>("DigitalDecodedPanes")!;

        var half = panes.Bounds.Width / 2;

        _output.WriteLine(
            "before, at *,*     : decoded " + Px(half) + " | For you " + Px(half));
        _output.WriteLine(
            "after,  at 383,*   : decoded " + Px(decoded.Bounds.Width)
            + " | For you " + Px(mine.Bounds.Width));
        _output.WriteLine(
            "the decoded list gives up " + Px(half - decoded.Bounds.Width)
            + " px and For you takes them");

        Assert.True(
            decoded.Bounds.Width < half,
            "the decoded list is " + Px(decoded.Bounds.Width)
            + " px, which is not narrower than the " + Px(half) + " that *,* gave it");

        Assert.True(
            mine.Bounds.Width > half,
            "For you is " + Px(mine.Bounds.Width)
            + " px, which is not wider than the " + Px(half) + " that *,* gave it");
    }

    /// <summary>How wide a line is drawn in the decoded row's own typeface.</summary>
    /// <remarks>
    /// **THE SAME FAMILY AND SIZE THE ROW DECLARES**, monospace at 12. The headless host
    /// has no Consolas and falls back to a face that advances 10.0 px a character, which
    /// is wider than the real one - so a column that satisfies this test has headroom on
    /// the glass rather than the other way round.
    /// </remarks>
    private static double Measure(string text)
        => new Avalonia.Media.FormattedText(
            text,
            CultureInfo.InvariantCulture,
            Avalonia.Media.FlowDirection.LeftToRight,
            new Avalonia.Media.Typeface(
                new Avalonia.Media.FontFamily("Consolas,Menlo,monospace")),
            12,
            Avalonia.Media.Brushes.Black).Width;

    private static string Px(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Box(Control control)
        => Px(control.Bounds.Width) + " x " + Px(control.Bounds.Height)
            + " at " + Px(control.Bounds.X) + "," + Px(control.Bounds.Y);

    /// <summary>How far <paramref name="control"/> sits inside <paramref name="frame"/>.</summary>
    private static double Offset(Control control, Control frame)
    {
        var y = 0.0;
        var walk = control.GetVisualParent();

        while (walk is not null && !ReferenceEquals(walk, frame))
        {
            if (walk is Control c)
            {
                y += c.Bounds.Y;
            }

            walk = walk.GetVisualParent();
        }

        return y;
    }

    /// <summary>The digital tab, realized, with rows on the left and a card on the right.</summary>
    /// <param name="width">How wide the window is, because the split is a measurement.</param>
    private static Window Realized(double width)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
            DigitalMineExpanded = true,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        // **THE LONGEST LINE, AND TWO ORDINARY ONES.** The column is sized to the worst
        // case and has to hold the common case in the same width.
        model.AddDecodeRowForTests(
            "021100", "-14", "0.1", "1240", LongestLine, Slot("02:11:00"), 14_074_000);

        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1310", "CQ DX " + Station + " " + FarGrid,
            Slot("02:11:15"), 14_074_000);

        // **AND A CONVERSATION, SO THE For you SIDE HAS A CARD TO MEASURE.**
        model.AddSentRowForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));
        model.RecordSentForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));

        model.AddDecodeRowForTests(
            "021130", "-09", "0.2", "1240", HisCall + " " + Station + " " + FarGrid,
            Slot("02:11:30"), 14_074_000);

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        var window = new MainWindow { DataContext = model, Width = width, Height = 1200 };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
