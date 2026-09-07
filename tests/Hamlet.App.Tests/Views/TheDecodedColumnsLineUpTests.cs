using Avalonia.Controls;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 241, task 2: the header sits over the columns it names.
/// </summary>
/// <remarks>
/// <para>**THE MECHANISM, NAMED IN TASK 1.** The header was one `Grid` with
/// `Auto,Auto,Auto,Auto,*` and every data row was a *separate* `Grid` with the
/// same string. Sibling grids share no measure: each `Auto` column sizes to its
/// own content, so the header's first column sized to `utc` at FontSize 11 while
/// a row's sized to `214135` at FontSize 12, and two rows carrying `231` and
/// `2438` disagreed with each other as well.</para>
/// <para>**SO THIS ASSERTS ORIGINS AND NOT APPEARANCE.** Nudging margins until a
/// screenshot looks right would hold for one font and one set of values. Column
/// origins that agree cannot be wrong for some other value, because there is no
/// measure left to disagree about.</para>
/// <para>**IT BUILDS THE REAL WINDOW HEADLESS**, following `BindingHealthTests`.
/// A test over the markup as text could not have caught this: both grids read
/// identically and were still measured apart.</para>
/// </remarks>
public sealed class TheDecodedColumnsLineUpTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the column origins are printed.</param>
    public TheDecodedColumnsLineUpTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The header's columns start where the rows' columns start.</summary>
    [AvaloniaFact]
    public void TheHeaderAndTheFirstRowShareColumnOrigins()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            // **THE DIGITAL WORKSPACE IS COLLAPSED UNLESS THIS TAB IS THE ONE
            // SHOWING**, so without it nothing under it is realized and the
            // test would look at an empty visual tree and pass by finding
            // nothing to disagree.
            OperatingMode = "Digital",

            // The panel's expand state is loaded from settings in the
            // constructor, so it is set here rather than relied on.
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = model };

        window.Show();

        // **THE ROWS ARRIVE AFTER `Show`, THROUGH THE REAL DECODE PATH, AND
        // BOTH HALVES OF THAT MATTER.** Showing the window raises `Opened`,
        // which starts the reconnect, which clears the decoded table - rows put
        // in beforehand are gone by the time anything is measured, which cost
        // this test three runs to find. And going through `ShowDecodes` rather
        // than adding to the collection is what raises `HasDigitalDecodes`, so
        // the header becomes visible and therefore gets arranged at all.
        Decode(model);

        // **A LAYOUT PASS IS NEEDED AS WELL AS THE JOBS.** Running the
        // dispatcher realizes the item containers; it does not necessarily
        // measure and arrange them, and a column origin only exists once
        // something has been arranged.
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var rows = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalDecodedRows");

        Assert.NotNull(rows);

        var header = window.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(g => g.Children.OfType<TextBlock>()
                .Any(t => t.Text == "utc"));

        Assert.NotNull(header);

        var rowGrids = rows.GetVisualDescendants()
            .OfType<Grid>()
            .ToList();

        _output.WriteLine("rows in the model after Show : "
            + model.DigitalDecodes.Count);
        _output.WriteLine("ItemsControl item count : " + rows.ItemCount);
        _output.WriteLine("descendants under it    : "
            + string.Join(", ", rows.GetVisualDescendants()
                .Select(d => d.GetType().Name).Distinct()));

        Assert.True(
            rowGrids.Count >= 2,
            "fewer than two rows were realized, so nothing could be compared "
            + "against anything");

        var headerOrigins = Origins(header);

        _output.WriteLine("header column origins : "
            + string.Join(", ", headerOrigins.Select(o => o.ToString("0.##"))));

        for (var i = 0; i < rowGrids.Count; i++)
        {
            var origins = Origins(rowGrids[i]);

            _output.WriteLine("row " + i + " column origins  : "
                + string.Join(", ", origins.Select(o => o.ToString("0.##"))));

            Assert.Equal(headerOrigins.Count, origins.Count);

            for (var column = 0; column < headerOrigins.Count; column++)
            {
                Assert.True(
                    Math.Abs(headerOrigins[column] - origins[column]) < 0.5,
                    "column " + column + " of row " + i + " starts at "
                    + origins[column].ToString("0.##")
                    + " and its header starts at "
                    + headerOrigins[column].ToString("0.##")
                    + " - the header is not over the column it names");
            }
        }

        window.Close();
    }

    /// <summary>
    /// The mine list's header sits over its own columns, which are not the left
    /// list's.
    /// </summary>
    /// <remarks>
    /// <para>**EACH HEADER AGAINST ITS OWN ROWS, SINCE THE TWO SIDES DIFFER**
    /// (work instruction 275 task 2). The left list dropped `dt` and `hz` on
    /// 2026-09-07 and the mine list kept them, so the sides carry different
    /// columns deliberately — and a test that compared one side's header with the
    /// other's rows would fail for the right reason and tell nobody
    /// anything.</para>
    /// <para>**THE HEADER IS FOUND BY `from`, WHICH ONLY THIS SIDE HAS.** The left
    /// list has no such column, so there is no way for this to pick up the wrong
    /// grid.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheMineHeaderAndItsRowsShareColumnOrigins()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = model, Width = 1400, Height = 1200 };

        window.Show();

        Decode(model);

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        _output.WriteLine("left rows : " + model.DigitalVisibleDecodes.Count);
        _output.WriteLine("mine rows : " + model.DigitalMineDecodes.Count);

        Assert.True(
            model.DigitalMineDecodes.Count >= 2,
            "the fixture put fewer than two rows on the mine side, so there is "
            + "nothing to compare");

        var rows = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == "DigitalMineRows");

        Assert.NotNull(rows);

        // **THE MINE HEADER IS THE ONE WITH TWO COLUMNS.** Both sides label a
        // `message` column, and since the `from` column went on 2026-09-07 there
        // is no heading unique to this side - so it is found by shape: `utc` and
        // `message` and nothing else.
        var header = window.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(g =>
                g.Children.OfType<TextBlock>().Any(t => t.Text == "utc")
                && g.Children.OfType<TextBlock>().Any(t => t.Text == "message")
                && g.Children.OfType<TextBlock>().Count() == 2);

        Assert.NotNull(header);

        var rowGrids = rows!.GetVisualDescendants().OfType<Grid>().ToList();

        Assert.True(
            rowGrids.Count >= 2,
            "fewer than two mine rows were realized");

        var headerOrigins = Origins(header!);

        _output.WriteLine("mine header origins : "
            + string.Join(", ", headerOrigins.Select(o => o.ToString("0.##"))));

        for (var i = 0; i < rowGrids.Count; i++)
        {
            var origins = Origins(rowGrids[i]);

            _output.WriteLine("mine row " + i + " origins  : "
                + string.Join(", ", origins.Select(o => o.ToString("0.##"))));

            // **THE ROW HAS A SECOND LINE THE HEADER DOES NOT.** The contact
            // text and the worked mark sit under the message, so a row can carry
            // a column the header has no cell for. What must hold is that every
            // column the header names starts where the row's does.
            for (var column = 0; column < headerOrigins.Count
                 && column < origins.Count; column++)
            {
                Assert.True(
                    Math.Abs(headerOrigins[column] - origins[column]) < 0.5,
                    "column " + column + " of mine row " + i + " starts at "
                    + origins[column].ToString("0.##")
                    + " and its header starts at "
                    + headerOrigins[column].ToString("0.##"));
            }
        }

        window.Close();
    }

    /// <summary>
    /// **THE `hz` COLUMN NO LONGER EXISTS ANYWHERE, AND THIS IS THE RECORD OF
    /// WHAT IT USED TO PROVE.**
    /// </summary>
    /// <remarks>
    /// <para>Unit 241 built this to hold one property: `hz` was right-aligned, so
    /// a three-digit tone and a four-digit one lined up on their units. **231 and
    /// 2438 are both real**, and left-aligned the hundreds digit of one sat under
    /// the thousands digit of the other, which is worse than no column because the
    /// eye compares them anyway.</para>
    /// <para>**IT HAS NO SUBJECT SINCE 2026-09-07.** Tim ruled `dt` and `hz` off
    /// the left list, choosing from five options with the numbers behind each, and
    /// work instruction 275 found they had never been on the mine side at all -
    /// unit 273 built that side as `utc`, `from`, `message`. So there is no `hz`
    /// cell in the application to align.</para>
    /// <para>**IT IS KEPT RATHER THAN DELETED, AND SKIPPED RATHER THAN LEFT RED.**
    /// Deleting it would destroy the record of a real fault and its fix; leaving it
    /// running would report a property nothing has. If `hz` ever returns, this is
    /// the test that was written for it and the reason it mattered.</para>
    /// </remarks>
    [Fact(Skip = "The hz column no longer exists on either list. See the summary.")]
    public void TheToneColumnAgreedOnItsUnitsWhileThereWasOne()
    {
    }

    private static List<double> Origins(Grid grid)
        => grid.Children
            .OfType<Control>()

            // **ONE VISIBLE CHILD PER COLUMN.** Since unit 241 task 5 the
            // message column holds two children - the three coloured fields and
            // the whole-message fallback - and exactly one of them is visible
            // for any given row. A hidden control is arranged at nought, so
            // taking every child would compare a real origin against a zero.
            .Where(c => c.IsVisible)
            .GroupBy(Grid.GetColumn)
            .OrderBy(g => g.Key)
            .Select(g => g.First().Bounds.X)
            .ToList();

    /// <summary>Put two real decodes on the table, at two tone widths.</summary>
    /// <remarks>
    /// **THROUGH THE DECODER, NOT AROUND IT.** Two messages are synthesised into
    /// one slot at 231 Hz and 2438 Hz - a three-digit tone and a four-digit one,
    /// which are exactly the two widths that used to give two different column
    /// origins.
    /// </remarks>
    private static void Decode(MainWindowViewModel model)
    {
        const int Rate = 48_000;

        var samples = new float[Rate * 30];

        // **TWO FOR EACH SIDE SINCE WORK INSTRUCTION 275.** The decoded area
        // split in two on 2026-09-07 and the two lists now carry DIFFERENT
        // columns - the left dropped `dt` and `hz`, the mine side kept them - so
        // each header has to be checked against its own rows and both sides need
        // rows to check.
        //
        // **THE TONE PAIR IS ON THE MINE SIDE**, because that is where `hz`
        // lives now. 231 and 2438 are a three-digit tone and a four-digit one,
        // which are exactly the two widths that used to give two different
        // column origins.
        Place(samples, Rate, "CQ", "TA3MPK", "KM39", 800f);
        Place(samples, Rate, "W4WTM", "K1ABC", "EM74", 1500f);
        Place(samples, Rate, "KC3QIS", "TA3MPK", "KM39", 231f);
        Place(samples, Rate, "KC3QIS", "K1ABC", "EM74", 2438f);

        model.ShowDecodes(
            new MonoAudio(Rate, samples),
            new DateTime(2026, 9, 4, 21, 41, 47, DateTimeKind.Utc),
            new ClockOffset(0, new DateTime(2026, 9, 4, 21, 40, 0, DateTimeKind.Utc)));
    }

    private static void Place(
        float[] samples, int rate, string to, string from, string payload, float hz)
    {
        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack(to, from, payload, message));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(message), rate, hz);

        // Thirteen seconds in, so the slot ends inside a recording that ends at
        // 21:41:47 - the same geometry the existing decoded-table test uses.
        var at = 13 * rate;

        for (var i = 0; i < slot.Length && at + i < samples.Length; i++)
        {
            samples[at + i] += slot[i];
        }
    }
}
