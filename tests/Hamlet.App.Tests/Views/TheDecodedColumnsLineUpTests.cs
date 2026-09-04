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
    /// The `hz` column is right-aligned, so a three-digit tone and a four-digit
    /// one line up on their units.
    /// </summary>
    /// <remarks>
    /// **231 AND 2438 ARE BOTH REAL AND THEY READ AS A COLUMN.** Left-aligned,
    /// the hundreds digit of one sits under the thousands digit of the other,
    /// and a column of numbers that cannot be compared at a glance is worse than
    /// no column, because the eye compares them anyway.
    /// </remarks>
    [AvaloniaFact]
    public void TheToneColumnAgreesOnItsUnits()
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

        // The two tones the fixture places, one three digits and one four.
        var wanted = model.DigitalDecodes.Select(r => r.Hz).ToHashSet(StringComparer.Ordinal);

        _output.WriteLine("tones decoded: " + string.Join(", ", wanted));

        Assert.True(
            wanted.Any(h => h.Length == 3) && wanted.Any(h => h.Length == 4),
            "the fixture did not produce both a three-digit and a four-digit "
            + "tone, so there is nothing to compare: " + string.Join(", ", wanted));

        var tones = window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(t => t.Text is not null && wanted.Contains(t.Text))
            .ToList();

        Assert.True(tones.Count >= 2, "fewer than two tone cells were realized");

        // **`Bounds` IS ALREADY RELATIVE TO THE PARENT GRID**, and every row
        // grid now starts at the same place, so the right edge inside the grid
        // is directly comparable between rows without translating anything.
        var rights = tones.Select(t => t.Bounds.Right).ToList();

        _output.WriteLine("right edges: "
            + string.Join(", ", rights.Select(r => r.ToString("0.##"))));

        Assert.All(
            rights,
            r => Assert.True(
                Math.Abs(r - rights[0]) < 0.5,
                "the tones end at " + string.Join(", ",
                    rights.Select(x => x.ToString("0.##")))
                + ", so they do not agree on their units"));

        window.Close();
    }

    /// <summary>Where each column starts, inside its own grid.</summary>
    /// <remarks>
    /// **`Bounds.X` IS THE COLUMN ORIGIN AND NEEDS NO TRANSLATION.** A child's
    /// bounds are already expressed in its parent's coordinates, and comparing
    /// origins inside each grid is exactly the question: does the header's
    /// column three start where the rows' column three starts.
    /// </remarks>
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

        Place(samples, Rate, "CQ", "TA3MPK", "KM39", 231f);
        Place(samples, Rate, "W4WTM", "K1ABC", "EM74", 2438f);

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
