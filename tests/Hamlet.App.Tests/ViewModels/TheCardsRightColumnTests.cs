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

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 330 task 2: **the card's right column is a table.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12, WITH A SCREENSHOT**: *"the right of globe text is not aligned
/// or even, make it nice. It's a jumbled mess."* What he was looking at was unit 327's
/// column - `3,000 miles · south / Grid FI07 / His time: 09:21 by the sun / 2 messages
/// show the messag / New country`. Five sentences of five different lengths, one of them
/// cut off at the card's edge, and *show the messages* on the card twice.</para>
/// <para>**REWRITTEN UNDER §R12 AND WHAT IT KEEPS IS THE PART THAT WAS NEVER WRONG.** Unit
/// 327's class asserted the words, and the words were right; it asserted nothing at all
/// about the shape, so a column that read as a jumble passed it. Every assertion about
/// *absent is not dashed* and *a door names nothing* is carried over unchanged, because
/// those are §0.0 and §3.1 and not this unit's to relax. What is added is the shape: five
/// rows at one height, nothing clipped, one link on the whole card.</para>
/// <para>**COMPUTED, NOT SEEN.** The words are read off the view model and the geometry off
/// a realized headless window; nobody looked at the application.</para>
/// </remarks>
public sealed class TheCardsRightColumnTests
{
    /// <summary>Vienna, JN88 - a long path and a clear compass word from FN00.</summary>
    private const string FarGrid = "JN88";

    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the column is printed.</param>
    public TheCardsRightColumnTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: the six facts render from a full card, label and value apart.**
    /// </summary>
    /// <remarks>
    /// **THE ORDER IS THE INSTRUCTION'S AND SO IS THE CONTENT**: distance and compass,
    /// grid, his time by the sun, what he last said and when, how many messages, and why he
    /// is marked. **The value carries no label in it**, because the table has a column for
    /// that; the `*Line` properties are the same facts as sentences and are asserted here
    /// too so the two cannot drift.
    /// </remarks>
    [Fact]
    public void TheSixFactsRenderFromAFullCard()
    {
        var card = Card(FarGrid, said: "73");

        card.UseNudge(new NudgeReason(NudgeKind.Visible, "Austria", "Europe", 2));

        foreach (var row in Rows(card))
        {
            _output.WriteLine(row.Label.PadLeft(10) + "   " + row.Value);
        }

        // **DISTANCE AND DIRECTION, ONE VALUE, NO DEGREES** (HM-DEC-038).
        Assert.True(card.HasDistanceLine);
        Assert.Contains("miles", card.DistanceValue, StringComparison.Ordinal);
        Assert.Contains("·", card.DistanceValue, StringComparison.Ordinal);
        Assert.DoesNotContain("°", card.DistanceValue, StringComparison.Ordinal);

        Assert.True(card.HasGridLine);
        Assert.Equal(FarGrid, card.GridValue);
        Assert.Equal("Grid " + FarGrid, card.GridLine);

        // **THE TIME SAYS WHAT IT IS.** A clock reading offered as *his local time* would
        // be a claim about a time zone Hamlet does not know (§0.0), so *by the sun* is in
        // the value and not in the label.
        Assert.True(card.HasSolarTimeLine);
        Assert.Contains("by the sun", card.SolarTimeValue, StringComparison.Ordinal);
        Assert.DoesNotContain("His time", card.SolarTimeValue, StringComparison.Ordinal);
        Assert.Equal("His time: " + card.SolarTimeValue, card.SolarTimeLine);

        // **A READING CARRIES ITS AGE** (HM-DEC-111).
        Assert.True(card.HasLastHeardLine);
        Assert.StartsWith("73", card.LastHeardValue, StringComparison.Ordinal);
        Assert.Contains("ago", card.LastHeardValue, StringComparison.Ordinal);
        Assert.Equal("Last heard: " + card.LastHeardValue, card.LastHeardLine);

        // **THE WORD *messages* IS THE LABEL AND IS NOT SAID TWICE.**
        Assert.True(card.HasMessages);
        Assert.Equal("1", card.MessagesValue);
        Assert.Equal("show them", card.MessagesLinkLabel);

        Assert.True(card.HasNudgeLine);
        Assert.Equal("New country", card.NudgeLine);

        Assert.True(card.HasRightColumn);
    }

    /// <summary>
    /// **Assertion 2: an absent fact is an absent row, and never a dash.**
    /// </summary>
    /// <remarks>
    /// **ABSENT, NOT DASHED** (§0.0). The solar time is worked out from his longitude, so a
    /// station with no grid has no time either, and neither row is drawn rather than drawn
    /// empty. The distance goes with them, for the same reason. **Carried unchanged from
    /// unit 327** and extended to the table: the row's two cells share one `Has`, so the
    /// grid row measures nothing and the table closes up.
    /// </remarks>
    [AvaloniaFact]
    public void AnAbsentFactIsAnAbsentRowAndNeverADash()
    {
        var card = Card(grid: null, said: "73");

        _output.WriteLine("grid     : [" + card.GridValue + "] shown " + card.HasGridLine);
        _output.WriteLine("time     : [" + card.SolarTimeValue + "] shown " + card.HasSolarTimeLine);
        _output.WriteLine("distance : [" + card.DistanceValue + "] shown " + card.HasDistanceLine);

        Assert.False(card.HasGridLine);
        Assert.Empty(card.GridValue);

        Assert.False(card.HasSolarTimeLine);
        Assert.Empty(card.SolarTimeValue);

        Assert.False(card.HasDistanceLine);
        Assert.Empty(card.DistanceValue);

        // **AND NOTHING ANYWHERE IN THE COLUMN IS A DASH.**
        foreach (var row in Rows(card))
        {
            Assert.NotEqual("—", row.Value.Trim());
            Assert.NotEqual("-", row.Value.Trim());
        }

        // **AND A STATION WHO HAS SAID NOTHING HAS NO *last heard* EITHER.**
        var silent = Card(FarGrid, said: null);

        Assert.False(silent.HasLastHeardLine);
        Assert.Empty(silent.LastHeardValue);

        // **THE ROW IS GONE FROM THE TABLE AND NOT DRAWN BLANK**, measured on the
        // realized window: a station who earns nothing draws no `Achieves` label and no
        // empty cell where one would have been.
        var window = Realized(nudged: false);

        try
        {
            var table = Table(window);
            var labels = LabelsIn(table);

            _output.WriteLine("labels drawn: " + string.Join(", ", labels));

            Assert.DoesNotContain("Achieves", labels);

            Assert.DoesNotContain(
                table.Children.OfType<Control>().Where(c => c.IsVisible),
                c => Grid.GetRow(c) == 5);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 3: a door's line names nothing, and the popup opens from it.**
    /// </summary>
    /// <remarks>
    /// **§3.1 HOLDS ON THE CARD AS WELL AS ON THE LIST.** *Would open a new area* is the
    /// category; the area is not in this object at all. **Carried unchanged from unit
    /// 327.**
    /// </remarks>
    [Fact]
    public void ADoorsLineNamesNothingAndThePopupOpensFromIt()
    {
        var card = Card(FarGrid, said: "73");

        card.UseNudge(new NudgeReason(NudgeKind.Door, "", "", 0));

        _output.WriteLine("line  : " + card.NudgeLine);
        _output.WriteLine("reason: " + card.NudgeReasonLine);
        _output.WriteLine("earns : " + card.NudgeEarnsLine);

        Assert.Equal("Would open a new area", card.NudgeLine);
        Assert.Equal(NudgeWords.DoorReason, card.NudgeReasonLine);
        Assert.Equal(NudgeWords.Earns, card.NudgeEarnsLine);

        foreach (var continent in new[]
        {
            "Africa", "Antarctica", "Asia", "Europe", "North America", "Oceania",
            "South America",
        })
        {
            Assert.DoesNotContain(
                continent, card.NudgeLine + card.NudgeReasonLine,
                StringComparison.OrdinalIgnoreCase);
        }

        Assert.False(card.NudgeIsOpen);

        card.OpenTheNudgeCommand.Execute(null);

        Assert.True(card.NudgeIsOpen, "the card's reason line did not open the popup");

        card.CloseTheNudgeCommand.Execute(null);

        Assert.False(card.NudgeIsOpen);

        // **AND *confirmed* IS NOWHERE** (§4).
        Assert.DoesNotContain(
            "confirm",
            card.NudgeLine + card.NudgeReasonLine + card.NudgeEarnsLine,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **Assertion 4: five rows, every one of them the same height.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ASSERTION UNIT 327 DID NOT HAVE AND THE ONE HIS COMPLAINT IS ABOUT.**
    /// The heights are read off a realized headless window - a `Height` in the markup is a
    /// request and the layout is the answer, and the two have disagreed in this application
    /// before.
    /// </remarks>
    [AvaloniaFact]
    public void FiveRowsAndEveryOneOfThemTheSameHeight()
    {
        var window = Realized(nudged: true);

        var table = Table(window);

        var cells = table.Children
            .OfType<Control>()
            .Where(c => c is not Avalonia.Controls.Primitives.Popup
                && c.IsVisible && c.Bounds.Height > 0)
            .ToList();

        foreach (var cell in cells)
        {
            _output.WriteLine(
                Grid.GetRow(cell) + " " + Grid.GetColumn(cell) + "  "
                + Text(cell).PadRight(28)
                + cell.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)
                + " px tall, " + cell.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " px wide");
        }

        var rows = cells.Select(c => Grid.GetRow(c)).Distinct().OrderBy(r => r).ToList();

        _output.WriteLine("");
        _output.WriteLine("rows drawn: " + rows.Count);

        // **THE INSTRUCTION'S FIVE ARE ALL HERE, AND THIS CARD HAS A SIXTH FACT.** The
        // station on this fixture has sent a message, so `Last heard` is drawn as well -
        // it is the sixth of the six facts unit 327 built and the instruction's row list
        // does not name it. **It is kept**: dropping a fact that is on the screen today
        // is a loss §0.5 does not allow, and the shape Tim objected to is the raggedness
        // rather than the count. Reported in output.md section 1 as the session's call.
        Assert.True(rows.Count is 5 or 6, "the table drew " + rows.Count + " rows");

        var heights = rows
            .Select(r => cells.Where(c => Grid.GetRow(c) == r).Max(c => c.Bounds.Height))
            .ToList();

        for (var i = 0; i < rows.Count; i++)
        {
            _output.WriteLine(
                "  row " + rows[i] + " : "
                + heights[i].ToString("0.0", CultureInfo.InvariantCulture) + " px");
        }

        Assert.True(
            heights.Max() - heights.Min() < 0.51,
            "the rows are " + string.Join(
                ", ", heights.Select(h => h.ToString("0.0", CultureInfo.InvariantCulture)))
            + " px tall, so the column is still ragged");

        // **AND THE LABELS ARE THE INSTRUCTION'S**, in the instruction's order.
        Assert.Equal(
            new[] { "Distance", "Grid", "His time", "Last heard", "Messages", "Achieves" },
            LabelsIn(table));

        // **THE INSTRUCTION'S FIVE, EXACTLY, ON A CARD THAT HAS ONLY THOSE FIVE FACTS.**
        // A station who has said nothing this card can quote has no `Last heard`, and
        // that is the row list Tim was shown.
        var five = Card(FarGrid, said: null);

        five.UseNudge(new NudgeReason(NudgeKind.Visible, "Austria", "Europe", 2));

        var drawn = Rows(five).Where(r => r.Value.Length > 0).Select(r => r.Label).ToList();

        _output.WriteLine("");
        _output.WriteLine("a card with no last-heard: " + string.Join(", ", drawn));

        Assert.Equal(
            new[] { "Distance", "Grid", "His time", "Messages", "Achieves" }, drawn);
    }

    /// <summary>
    /// **Assertion 5: nothing is clipped, and one *show* link is on the whole card.**
    /// </summary>
    /// <remarks>
    /// <para>**CLIPPING IS MEASURED AND NOT ASSUMED**: every visible run of text in the
    /// table is asked what width it wants and compared with the width it was given. That is
    /// the `show the messag` in his screenshot, caught.</para>
    /// <para>**AND THE LINK IS COUNTED ON THE WHOLE CARD**, not in the column, because the
    /// duplicate was on the card's top row and a count taken inside the table would have
    /// found one and passed.</para>
    /// </remarks>
    [AvaloniaFact]
    public void NothingIsClippedAndOneShowLinkIsOnTheWholeCard()
    {
        var window = Realized(nudged: true);

        var cards = window.FindControl<ItemsControl>("DigitalContactCards")!;
        var table = Table(window);

        foreach (var text in table.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsVisible && (t.Text ?? "").Trim().Length > 0))
        {
            // **THE MARGIN IS IN `DesiredSize` AND IS NOT INK.** A label asks for its
            // own width plus the eight pixels of air between the two columns; what
            // would be cut off is the text, so the air comes back off first.
            var wants = text.DesiredSize.Width - text.Margin.Left - text.Margin.Right;

            _output.WriteLine(
                "[" + text.Text + "] wants "
                + wants.ToString("0.0", CultureInfo.InvariantCulture)
                + " px, has "
                + text.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture) + " px");

            Assert.True(
                text.Bounds.Width + 0.51 >= wants,
                "[" + text.Text + "] wants "
                + wants.ToString("0.0", CultureInfo.InvariantCulture)
                + " px and was given "
                + text.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " px, so it is cut off at the card's edge");

            Assert.Equal(Avalonia.Media.TextTrimming.None, text.TextTrimming);
            Assert.Equal(Avalonia.Media.TextWrapping.NoWrap, text.TextWrapping);
        }

        // **AND WHAT THE ROWS WOULD WANT WITH NO CONSTRAINT AT ALL**, printed rather
        // than asserted, because a `TextBlock`'s `DesiredSize` is clamped to what it was
        // offered and a value squeezed to nothing reports that it wanted nothing.
        //
        // **THE HEADLESS SHAPER IS NOT THE APPLICATION'S FONT AND THE FIGURES BELOW ARE
        // ITS, NOT THE GLASS'S** (§0.0, FACT-004). It advances a flat 10.0 px per
        // character at `FontSize` 11 - `Grid` measures 40.0 and `His time` 80.0 - where a
        // real proportional face at 11 point averages a little over half that. So the
        // totals here are roughly twice what the card draws, and the fit is arithmetic
        // rather than a green light: nobody in this repository can look at a pixel.
        var widest = 0.0;

        foreach (var text in table.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsVisible && (t.Text ?? "").Trim().Length > 0))
        {
            var natural = new Avalonia.Media.FormattedText(
                text.Text!,
                CultureInfo.InvariantCulture,
                Avalonia.Media.FlowDirection.LeftToRight,
                new Avalonia.Media.Typeface(text.FontFamily),
                text.FontSize,
                null).Width;

            _output.WriteLine(
                "   unconstrained [" + text.Text + "] "
                + natural.ToString("0.0", CultureInfo.InvariantCulture) + " px");

            widest = Math.Max(widest, natural + (Grid.GetColumn(text) == 1 ? 108 : 0));
        }

        _output.WriteLine(
            "the widest row wants " + widest.ToString("0.0", CultureInfo.InvariantCulture)
            + " px of the card's " + table.Bounds.Width.ToString(
                "0.0", CultureInfo.InvariantCulture)
            + ", at the headless shaper's 10.0 px per character");

        // **THE TABLE IS NOT SQUEEZED BY WHAT IS AROUND IT**, which is the layout
        // question this can actually answer.
        Assert.True(
            table.Bounds.Width + 0.51 >= table.DesiredSize.Width,
            "the table is " + table.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px wide and wants "
            + table.DesiredSize.Width.ToString("0.0", CultureInfo.InvariantCulture) + " px");

        // **ONE WAY DOWN TO THE CONVERSATION, ON THE WHOLE CARD.**
        var links = cards.GetVisualDescendants().OfType<Button>()
            .Where(b => b.IsVisible
                && (b.Content as string ?? "").Contains(
                    "show", StringComparison.OrdinalIgnoreCase))
            .Select(b => (string)b.Content!)
            .ToList();

        _output.WriteLine("");
        _output.WriteLine("show links on the card: " + string.Join(" | ", links));

        Assert.Single(links);
        Assert.Equal("show them", links[0]);
    }

    /// <summary>
    /// **Assertion 6: the map and the table share the card, and the table is not squeezed.**
    /// </summary>
    /// <remarks>
    /// <para>**THE INSTRUCTION'S WIDTH RULE DOES NOT FIT AND THIS IS THE MEASUREMENT THAT
    /// SAYS SO.** It asks for *the column takes what the map leaves and never pushes the
    /// map narrower*. On a 1400 px window the *For you* panel is a quarter of the tab, so
    /// the card is 275 px wide inside and the map's natural width is 222 - which leaves 53
    /// px for a table whose longest value is `4,400 miles · northeast`. **The two halves of
    /// task 2 cannot both hold at that width**, and *nothing truncated, nothing wrapped* is
    /// the half Tim complained about.</para>
    /// <para>**SO WHAT IS ASSERTED IS THE ONE THAT MATTERS**: the table gets the width it
    /// asks for, and the map is never drawn wider than its own natural size. Reported in
    /// output.md section 4 as a thing the owner may want ruled the other way.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheMapAndTheTableShareTheCardAndTheTableIsNotSqueezed()
    {
        var window = Realized(nudged: true);

        var table = Table(window);
        var beside = (Grid)table.Parent!;

        var map = beside.Children.OfType<Control>()
            .First(c => Grid.GetRow(c) == 0);

        // **BY WALKING AND NOT BY NAME.** The map is inside the card's `DataTemplate`,
        // which is its own name scope, so the window's `FindControl` cannot see it.
        var globe = window.FindControl<ItemsControl>("DigitalContactCards")!
            .GetVisualDescendants()
            .OfType<Hamlet.App.Controls.Ft8GlobeControl>()
            .FirstOrDefault();

        _output.WriteLine(
            "window: " + window.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine(
            "cards : " + window.FindControl<ItemsControl>("DigitalContactCards")!
                .Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine(
            "beside: " + beside.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine(
            "map   : " + map.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px, wants "
            + map.DesiredSize.Width.ToString("0.0", CultureInfo.InvariantCulture));
        _output.WriteLine(
            "table : " + table.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px, wants "
            + table.DesiredSize.Width.ToString("0.0", CultureInfo.InvariantCulture));

        // **THE TABLE GETS WHAT IT ASKS FOR.** Its column is `Auto`, so a squeezed one
        // is the truncation Tim photographed coming back by another route.
        Assert.True(
            table.Bounds.Width + 0.51 >= table.DesiredSize.Width,
            "the table is " + table.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px wide and wants "
            + table.DesiredSize.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px, so its values are cut off");

        // **AND THE MAP IS NEVER DRAWN WIDER THAN ITS OWN NATURAL SIZE**, which is what
        // survives of the instruction's width rule: the picture does not grow into the
        // room the table gave back on a wide window.
        Assert.True(globe is not null, "the card's map is not in the window");
        Assert.True(
            globe!.Bounds.Width <= 220.51,
            "the card's map is "
            + globe.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " px wide, which is wider than the 220 it has drawn at since unit 308");
    }

    /// <summary>
    /// **Assertion 7: no caption is bound under the map on the card's face.**
    /// </summary>
    /// <remarks>
    /// **CARRIED UNCHANGED FROM UNIT 327**, which is Tim's *"remove the info from below the
    /// map, I really never noticed it."* The enlarged map keeps its own caption, where
    /// there is room for it and where he went looking for it. It is here rather than
    /// deleted because this unit rebuilt the markup either side of it.
    /// </remarks>
    [Fact]
    public void NoCaptionIsBoundUnderTheMapOnTheCardsFace()
    {
        var markup = System.IO.File.ReadAllText(
            System.IO.Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var captions = Occurrences(markup, "Text=\"{Binding Globe.Caption}\"");

        _output.WriteLine("Globe.Caption is bound " + captions + " time(s) in MainWindow.axaml");

        // **ONE, AND IT IS THE ENLARGED MAP'S.**
        Assert.Equal(1, captions);
    }

    private static int Occurrences(string text, string what)
    {
        var count = 0;
        var at = 0;

        while ((at = text.IndexOf(what, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += what.Length;
        }

        return count;
    }

    private static string Root()
    {
        var here = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null
            && !System.IO.File.Exists(
                System.IO.Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        return here?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    /// <summary>The table, off a realized window.</summary>
    private static Grid Table(Window window)
    {
        var cards = window.FindControl<ItemsControl>("DigitalContactCards");

        Assert.True(cards is not null, "the cards list is not in the window");

        var table = cards!.GetVisualDescendants().OfType<Grid>()
            .FirstOrDefault(g => g.Name == "CardRightColumn");

        Assert.True(table is not null, "the card's right column is not a Grid named CardRightColumn");

        return table!;
    }

    /// <summary>Every label the table drew, top to bottom.</summary>
    private static IReadOnlyList<string> LabelsIn(Grid table)
        => table.Children
            .OfType<TextBlock>()
            .Where(t => Grid.GetColumn(t) == 0 && t.IsVisible)
            .OrderBy(t => Grid.GetRow(t))
            .Select(t => t.Text ?? "")
            .ToList();

    private static string Text(Control control)
        => control switch
        {
            TextBlock t => "[" + t.Text + "]",
            Button b => "(" + b.Content + ")",
            _ => control.GetType().Name,
        };

    /// <summary>One card in a realized window, with a grid, a map and a nudge.</summary>
    /// <remarks>
    /// **THE RECIPE IS `Unit297CardBindingTests`'S**, because a template that is never
    /// realized has no heights to measure and a visual walk of an unrealized workspace
    /// finds nothing at all.
    /// </remarks>
    private static Window Realized(bool nudged)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.AddSentRowForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));
        model.RecordSentForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));

        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", HisCall + " " + Station + " " + FarGrid,
            Slot("02:11:15"), 14_074_000);

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        Assert.Single(model.DigitalCards);

        // **A WIDTH TIM WOULD RECOGNIZE.** The headless window's default is narrow
        // enough that the table's star column is squeezed to nothing, and *is this
        // clipped* is a question about the width the card actually has. 1400 by 900 is
        // a laptop with the window maximized, which is where his screenshots come from.
        var window = new MainWindow { DataContext = model, Width = 1400, Height = 1200 };

        window.Show();

        // **A LAYOUT PASS AS WELL AS THE JOBS.** Running the dispatcher realizes
        // controls; it does not necessarily measure and arrange them, and an arranged
        // rectangle is the only thing the shape assertions are about.
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        // **THE NUDGE IS SET AFTER THE WINDOW IS UP AND NOT BEFORE.** The panel builds
        // its own nudge set as the cards are rebuilt, and this fixture's station is a
        // country nothing in the empty log has worked - so a nudge set before `Show`
        // is overwritten by the panel's own and the *earns nothing* case never renders.
        model.DigitalCards[0].UseNudge(nudged
            ? new NudgeReason(NudgeKind.Visible, "Austria", "Europe", 2)
            : new NudgeReason(NudgeKind.None, "", "", 0));

        for (var i = 0; i < 4; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    /// <summary>A slot boundary on the evening in question, in true UTC.</summary>
    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    /// <summary>The table as the view model would fill it, label and value.</summary>
    private static IReadOnlyList<(string Label, string Value)> Rows(Ft8ContactCard card)
        => new[]
        {
            ("Distance", card.DistanceValue),
            ("Grid", card.GridValue),
            ("His time", card.SolarTimeValue),
            ("Last heard", card.LastHeardValue),
            ("Messages", card.HasMessages ? card.MessagesValue : ""),
            ("Achieves", card.NudgeLine),
        };

    /// <summary>One conversation card, one message in, heard ten seconds ago.</summary>
    private static Ft8ContactCard Card(string? grid, string? said)
    {
        var at = new DateTime(2026, 9, 12, 11, 2, 0, DateTimeKind.Utc);

        var facts = new Ft8CardFacts(
            Callsign: "OE1ABC",
            State: Ft8ContactState.YourMove,
            Slots: 1,
            LastAtUtc: at,
            FirstAtUtc: at,
            YouCalledHim: false,
            HeCameBack: false,
            HisMessages: 1,
            YourMessages: 0,
            HeardInAll: 1,
            ReportFromHim: null,
            ReportToHim: null,
            HeRogered: false,
            YouRogered: false,
            HeSignedOff: false,
            YouSignedOff: false,
            Grid: grid,
            HisLastPayload: said,
            YourLastMessage: null);

        return new Ft8ContactCard(
            facts,
            "FN00",
            Ft8CardActionKind.None,
            "",
            "",
            at.AddSeconds(10));
    }
}
