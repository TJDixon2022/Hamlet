using System.Globalization;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 259, tasks 3 and 4: **the right-click reaches the send path,
/// and the reserved area says what is going out.**
/// </summary>
/// <remarks>
/// <para>**IT NEVER OPENS A WINDOW, A PORT OR A DEVICE.** Everything a reader
/// sees is formatted in the view model - the precedent <c>DigitalDecodeRow</c>
/// and unit 258's contact cell set - so the menu and the Send area can be
/// asserted as strings. **The transmission itself is proved in the engine**, on
/// the fake port and the fake sink, by
/// <c>OneClickSendsExactlyOneMessageTests</c>; this proves the join.</para>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT.** A send command that does
/// nothing at all when there is no radio, leaving the operator clicking a menu
/// item and watching an unchanged screen. On this machine no radio has ever been
/// attached (`SHACK_FACTS.md` FACT-004) and <c>Ic7300Rig</c> exposes no port, so
/// **the ordinary state of this command is refusal - and a refusal has to say
/// so.**</para>
/// <para>**AND THE SECOND: a menu that quietly withholds something.** The row's
/// menu is what <c>Ft8SendOptions</c> returned and there is no filtering step
/// between the two.</para>
/// </remarks>
public sealed class OneClickArmsExactlyOneMessageTests
{
    /// <summary>The boundary slot 0 opened on, as the scene has it.</summary>
    private static readonly DateTime SlotZero =
        new(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public OneClickArmsExactlyOneMessageTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **The reserved area says nothing has been sent, until something is.**
    /// </summary>
    /// <remarks>
    /// Step 5's criterion 5. The sentence it replaced - *"Send lives here when it
    /// is built. Hamlet does not transmit yet"* - became false tonight and is
    /// gone from the markup.
    /// </remarks>
    [Fact]
    public void TheSendAreaSaysNothingHasBeenSentBeforeAnythingIs()
    {
        var (panel, _) = Panel();

        _output.WriteLine(panel.DigitalSendLine);

        Assert.Equal(MainWindowViewModel.NothingHasBeenSent, panel.DigitalSendLine);
        Assert.DoesNotContain(
            "does not transmit", panel.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **With no radio and no named endpoint the send refuses with words.**
    /// </summary>
    /// <remarks>
    /// **NOT SILENCE.** Work instruction 259 task 3 names a command that
    /// silently does nothing as the one unacceptable landing, and names this one
    /// as acceptable and to be reported as such.
    /// </remarks>
    [Fact]
    public void WithNoRadioTheSendRefusesWithWordsRatherThanDoingNothing()
    {
        var (panel, _) = Panel();

        panel.SendMessageCommand.Execute("W1ABC KC3QIS -10");

        _output.WriteLine(panel.DigitalSendLine);

        Assert.NotEqual(MainWindowViewModel.NothingHasBeenSent, panel.DigitalSendLine);
        Assert.Contains("sent nothing", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains("no radio is connected", panel.DigitalSendLine, StringComparison.Ordinal);
        Assert.Contains("W1ABC KC3QIS -10", panel.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>**A message the composer refuses is said, not swallowed.**</summary>
    [Fact]
    public void AMessageThatWillNotPackIsSaidRatherThanSwallowed()
    {
        var (panel, _) = Panel();

        panel.SendMessageCommand.Execute("");

        _output.WriteLine(panel.DigitalSendLine);

        Assert.Contains("nothing to send", panel.DigitalSendLine, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **The row's menu is every message, with the expected one marked.**
    /// </summary>
    /// <remarks>
    /// Step 5's criterion 2, arriving on a row. The engine's own
    /// <c>TheMenuOffersEveryValidMessageTests</c> is what proves the sets against
    /// the corpus; this proves the row reaches them.
    /// </remarks>
    [Fact]
    public void TheRowsMenuOffersEveryMessageWithTheExpectedOneMarked()
    {
        var (panel, settings) = Panel();

        settings.Operator.GridSquare = "FN00";

        Add(panel, 6, "CQ W1ABC FN42", snr: "-14");

        var row = Add(panel, 8, "KC3QIS W1ABC R-15", snr: "-14");
        var menu = panel.SendMenuFor(row);

        Assert.NotNull(menu);

        foreach (var option in menu!.Options)
        {
            _output.WriteLine(
                option.Text + " | " + option.Label + " | "
                + (option.IsExpected ? "expected" : "-") + " | " + option.SentBefore);
        }

        Assert.Equal(5, menu.Options.Count);

        // THE REPORT IS THE ROW'S OWN MEASURED RATIO, not an invented figure.
        Assert.Contains(menu.Options, o => o.Text == "W1ABC KC3QIS -14");
        Assert.Contains(menu.Options, o => o.Text == "W1ABC KC3QIS FN00");
        Assert.Contains(menu.Options, o => o.Text == "W1ABC KC3QIS 73");

        // He rogered and reported, so an acknowledgement is what comes next.
        var expected = Assert.Single(menu.Options, o => o.IsExpected);

        Assert.Equal("W1ABC KC3QIS RRR", expected.Text);
    }

    /// <summary>
    /// **A row whose ratio was not measured loses the report messages and no
    /// number is invented.**
    /// </summary>
    [Fact]
    public void ARowWithNoMeasuredRatioOffersNoReportAndSaysWhy()
    {
        var (panel, settings) = Panel();

        settings.Operator.GridSquare = "FN00";

        var row = Add(
            panel, 2, "KC3QIS N5TT EM10", snr: DigitalDecodeRow.NoMeasurement);

        var menu = panel.SendMenuFor(row)!;

        _output.WriteLine(string.Join(", ", menu.Options.Select(o => o.Text)));
        _output.WriteLine(string.Join(" / ", menu.Absent));

        Assert.DoesNotContain(
            menu.Options,
            o => o.Shape is Ft8SendShape.Report or Ft8SendShape.RogerAndReport);

        Assert.Contains(menu.Absent, r => r.Contains("report", StringComparison.Ordinal));
    }

    private static (MainWindowViewModel Panel, AppSettings Settings) Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        return (new MainWindowViewModel(settings, null), settings);
    }

    private static DigitalDecodeRow Add(
        MainWindowViewModel panel, int slot, string message, string snr = "-11")
    {
        var utc = SlotZero.AddSeconds(slot * 15);

        return panel.AddDecodeRowForTests(
            utc.ToString("HHmmss", CultureInfo.InvariantCulture),
            snr, "0.2", "1240", message, utc);
    }
}
