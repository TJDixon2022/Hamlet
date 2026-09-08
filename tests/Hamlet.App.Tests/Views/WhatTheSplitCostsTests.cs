using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 273, task 5: what the split costs, measured rather than
/// assumed.
/// </summary>
/// <remarks>
/// <para>**THE QUESTION IS WHETHER THE MESSAGE STILL FITS.** Halving a list
/// halves its message column, and a column that is a few pixels short truncates
/// or wraps a callsign — which the operator would discover on the air, reading a
/// station that is not the one he thinks. **Do not shrink a column to make the
/// split fit and leave him to find out**, which is the instruction's own rule, so
/// this measures and reports and changes nothing.</para>
/// <para>**THE LONG CASES ARE HIS OWN.** `CQ W4/YV7AXM`, `IS0/IK2YCW` and a
/// compound callsign carrying a report are the longest real messages in his
/// captures. A standard FT8 message cannot exceed thirteen characters a field, so
/// the widest thing either list can ever be handed is bounded and is measured here
/// too.</para>
/// </remarks>
public sealed class WhatTheSplitCostsTests
{
    /// <summary>The row font, which is what the message is drawn in.</summary>
    private const double RowFontSize = 12;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the widths are printed.</param>
    public WhatTheSplitCostsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What each list is left with, and whether the message fits.</summary>
    [AvaloniaFact]
    public void TheMessageColumnStillFitsTheLongestRealMessage()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
            DigitalWaterfallExpanded = true,
            DigitalDecodedExpanded = true,
        };

        // Rows on both sides, so neither list is measuring an empty box.
        model.AddDecodeRowForTests("214135", "-11", "0.2", "1240", "CQ W4/YV7AXM");
        model.AddDecodeRowForTests("214135", "-09", "0.2", "1290", "KC3QIS IS0/IK2YCW -12");

        // The same window unit 251 measured the outer split at.
        var window = new MainWindow
        {
            DataContext = model,
            Width = 1400,
            Height = 1200,
        };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        var panes = Named<Grid>(window, "DigitalDecodedPanes");
        var left = Named<CollapsiblePanel>(window, "DigitalDecodedPanel");
        var mine = Named<CollapsiblePanel>(window, "DigitalMinePanel");

        var panesRect = RectIn(window, panes);
        var leftRect = RectIn(window, left);
        var mineRect = RectIn(window, mine);

        _output.WriteLine("window 1400 x 1200");
        _output.WriteLine("decoded area : " + Describe(panesRect));
        _output.WriteLine("  left list  : " + Describe(leftRect));
        _output.WriteLine("  mine list  : " + Describe(mineRect));

        // **THE TWO ARE THE SAME WIDTH**, which is what `*,*` gives and what a
        // pair of percentages could round apart.
        Assert.True(leftRect.Width > 0 && mineRect.Width > 0, "a list has no width");
        Assert.True(
            Math.Abs(leftRect.Width - mineRect.Width) <= 1,
            "the two lists differ by " + Math.Abs(leftRect.Width - mineRect.Width)
            + " pixels");

        // **WHAT IS LEFT FOR THE MESSAGE**, which is the list minus the fixed
        // columns in front of it.
        //
        // **THE LEFT LIST LOST `dt` AND `hz` ON 2026-09-07** (Tim, from five
        // options with the numbers behind each), so it went from
        // `76,48,48,54,Auto,*` to `76,48,Auto,*`.
        //
        // **AND IT LOST THE `Auto` ON 2026-09-08**, so it now declares `76,48,*`.
        // That column held unit 274's green `worked`, and Tim's ruling of that
        // day removed the word and fades the whole row instead, so there is
        // nothing left for the column to hold.
        //
        // **THE ARITHMETIC HERE DOES NOT CHANGE, AND THAT IS THE POINT WORTH
        // KNOWING.** `Auto` took no width on a row with nothing to say, and the
        // rows measured here have nothing to say, so `LeftFixed` was already
        // `76 + 48` before the column came out. **The width the message gets back
        // is therefore nought on these rows** - what the removal buys is the
        // width the mark took on the rows that *did* carry it, which is every row
        // for a station already in the log.
        //
        // **THE MINE LIST LOST ITS `from` COLUMN ON 2026-09-07**, so it declares
        // `76,*` where it declared `76,62,*`. That column was the only one on
        // either side whose content had no bound - a six-character callsign
        // pushed the row past its header - and it duplicated the message beside
        // it, which already opens with the sender.
        //
        // **AND NOTE WHAT THE INSTRUCTION EXPECTED VERSUS WHAT WAS THERE.** Task 1
        // says the mine list keeps `dt` and `hz`. It never had them: unit 273
        // built that side as `utc`, `from`, `message`.
        const double LeftFixed = 76 + 48;
        const double MineFixed = 76;

        // The panel's own padding and the scroller, taken off both. Measured from
        // the panel rectangle rather than assumed, so a theme change moves it.
        var leftBody = BodyWidth(window, "DigitalDecodedRows", leftRect);
        var mineBody = BodyWidth(window, "DigitalMineRows", mineRect);

        var leftMessage = leftBody - LeftFixed;
        var mineMessage = mineBody - MineFixed;

        _output.WriteLine("");
        _output.WriteLine(
            "left  message column : " + leftMessage.ToString("0") + " px");
        _output.WriteLine(
            "mine  message column : " + mineMessage.ToString("0") + " px");
        _output.WriteLine("");

        // **THE LONGEST REAL MESSAGES, AND THE WIDEST ONE THE FORMAT ALLOWS.**
        var cases = new[]
        {
            "CQ W4/YV7AXM",
            "IS0/IK2YCW",
            "KC3QIS IS0/IK2YCW -12",
            "W4/YV7AXM KC3QIS R-15",
            "VP2MAA KC3QIS FN00",
            // Thirteen characters is the most a standard callsign field carries,
            // so this is the widest a standard message can be.
            "W4/YV7AXM/QRP W4/YV7AXM/QRP R-15",
        };

        var worst = 0.0;
        var worstText = "";

        foreach (var text in cases)
        {
            var width = Measure(text);

            _output.WriteLine(
                "  " + width.ToString("0").PadLeft(4) + " px  " + text);

            if (width > worst)
            {
                worst = width;
                worstText = text;
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            "widest measured : " + worst.ToString("0") + " px  (" + worstText + ")");
        _output.WriteLine(
            "left  headroom  : " + (leftMessage - worst).ToString("0") + " px");
        _output.WriteLine(
            "mine  headroom  : " + (mineMessage - worst).ToString("0") + " px");

        // **WHAT THE ROW DOES WITH A MESSAGE TOO WIDE FOR IT IS A FACT ABOUT
        // THE MARKUP, AND IT IS NOT THE SAME ON BOTH SIDES.** The mine row's
        // message declares `TextWrapping="Wrap"`, so it grows a line rather than
        // losing a character; the left row declares none, so it clips. A probe of
        // the rendered cells was written here and removed: the rows are
        // virtualized and the headless host does not realize them, so it reported
        // `not rendered` rather than an answer, and a line that looks like a
        // measurement and is not is worse than no line (§0.0).

        // **NOT AN ASSERTION THAT IT FITS.** The instruction says measure and
        // report, and says plainly what to do if it does not: say so and say what
        // would have to give. A failing assertion here would be this session
        // deciding the answer instead of reporting it. What IS asserted is that
        // the measurement happened at all, so a silently empty layout cannot pass
        // as a comfortable fit.
        Assert.True(leftMessage > 0, "the left message column measured no width");
        Assert.True(mineMessage > 0, "the mine message column measured no width");
        Assert.True(worst > 0, "nothing was measured");
    }

    /// <summary>How wide a message is when drawn in the row font.</summary>
    /// <remarks>
    /// **THE SAME TYPEFACE THE ROWS DECLARE**, monospace at 12, because a
    /// proportional measurement of a monospace column would be the wrong number
    /// and would flatter the fit.
    /// </remarks>
    private static double Measure(string text)
        => new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Consolas,Menlo,monospace")),
            RowFontSize,
            Brushes.Black).Width;

    /// <summary>The width the rows themselves are given inside a panel.</summary>
    /// <remarks>
    /// Measured off the items control rather than off the panel, so the panel's
    /// padding, its border and the scroll bar are all taken off by the layout
    /// instead of being guessed at here.
    /// </remarks>
    private static double BodyWidth(Visual window, string rowsName, Rect fallback)
    {
        var rows = window.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(c => c.Name == rowsName);

        return rows is { Bounds.Width: > 0 } ? rows.Bounds.Width : fallback.Width;
    }

    private static T Named<T>(Visual root, string name)
        where T : Control
    {
        var found = root.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "no " + typeof(T).Name + " called " + name + " is in the window");

        return found!;
    }

    private static Rect RectIn(Visual window, Control control)
    {
        var origin = control.TranslatePoint(new Point(0, 0), window);

        Assert.True(
            origin.HasValue,
            "the control " + control.Name + " is not connected to the window");

        return new Rect(origin!.Value, control.Bounds.Size);
    }

    private static string Describe(Rect r)
        => "x=" + r.X.ToString("0") + " y=" + r.Y.ToString("0")
           + " w=" + r.Width.ToString("0") + " h=" + r.Height.ToString("0");
}
