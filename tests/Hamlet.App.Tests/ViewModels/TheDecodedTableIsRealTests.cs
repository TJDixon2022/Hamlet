using System.Globalization;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The Digital tab's decoded table shows what was decoded, and nothing else.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 037 BUILT THAT TABLE OUT OF FOUR LITERAL STRINGS AND
/// SAID SO IN ITS OWN MARKUP**: nothing decoded, nothing moved, nothing was
/// wired, and it existed so the operator could argue with a finished-looking FT8
/// session before there was a decoder to argue with. There is one now.</para>
/// <para>**BOTH HALVES ARE ASSERTED IN ONE FILE ON PURPOSE.** That the real rows
/// arrive, and that the invented ones are gone. Either one alone can pass while
/// the tab shows a mixture, which is worse than showing neither.</para>
/// <para>**NOTHING HERE OPENS A WINDOW OR TOUCHES A SOUND CARD.** A recording is
/// built in an array and handed to the view model, and the markup is read as
/// text.</para>
/// </remarks>
public sealed class TheDecodedTableIsRealTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheDecodedTableIsRealTests(ITestOutputHelper output) => _output = output;

    /// <summary>The four messages the markup used to assert had been heard.</summary>
    private static readonly string[] Invented =
    {
        "W9XYZ K1ABC -13",
        "CQ K1ABC FN42",
        "CQ DX EA3QQ JN11",
        "VE7AA N0RR RR73",
    };

    /// <summary>A clock that has been checked and found to match UTC.</summary>
    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>
    /// **NOTHING IS SHOWN BEFORE ANYTHING IS HEARD, AND THE PANEL SAYS WHY**
    /// (HM-DEC-021).
    /// </summary>
    [Fact]
    public void AnUntouchedTabShowsItsIdleLineAndNoRows()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        _output.WriteLine($"  rows {model.DigitalDecodes.Count}");
        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");
        _output.WriteLine($"  idle [{model.DigitalDecodedIdle}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);
        Assert.Equal(DigitalIdleText.Decoded, model.DigitalDecodedIdle);
    }

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** Audio with a transmission in it goes in,
    /// and the row that comes out carries the message that was sent.
    /// </summary>
    [Fact]
    public void AudioWithATransmissionInItFillsTheTable()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            Recording(48000),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            Measured);

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                $"  {row.Utc}  {row.Snr}  {row.Dt}  {row.Hz}  {row.Message}");
        }

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        var only = Assert.Single(model.DigitalDecodes);

        Assert.Equal("CQ K1ABC FN42", only.Message);
        Assert.Equal("142230", only.Utc);

        // Placed at 1240 Hz. The search answers in bins a fraction of a tone
        // wide, so the column is asserted as a neighbourhood and not a number.
        Assert.InRange(int.Parse(only.Hz, CultureInfo.InvariantCulture), 1236, 1244);

        // **THE SNR COLUMN CARRIES A DASH AND NOT A NUMBER** (§0.0). This library
        // returns a sync score and no decibels, and a plausible figure under that
        // heading would be read as a measurement.
        Assert.Equal(DigitalDecodeRow.NoMeasurement, only.Snr);

        Assert.True(model.HasDigitalDecodes);
        Assert.Equal("142230 UTC · 1 shown", model.DigitalDecodedSummary);
    }

    /// <summary>
    /// **A CLOCK NOBODY HAS CHECKED IS SAID IN WORDS**, on the panel's own
    /// summary, rather than left as an empty table that looks like a dead band.
    /// </summary>
    [Fact]
    public void AnUnmeasuredClockLeavesTheTableEmptyAndTheSummarySpeaking()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            Recording(48000),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            ClockOffset.Unknown);

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);
        Assert.Equal(Ft8SlotCutter.NoOffset, model.DigitalDecodedSummary);
    }

    /// <summary>
    /// **AUDIO THAT DECODED NOTHING SAYS SO RATHER THAN GOING QUIET.** Silence
    /// through the whole path leaves the table empty and the summary reporting
    /// that a slot was read and nobody was in it.
    /// </summary>
    [Fact]
    public void SilenceLeavesTheTableEmptyAndTheSummaryReportingTheSlot()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            new MonoAudio(48000, new float[48000 * 30]),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            Measured);

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);
        Assert.Contains("one slot", model.DigitalDecodedSummary, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE FOUR INVENTED ROWS ARE OUT OF THE MARKUP.** Read as text, because
    /// the assertion is about what a reader of the file would find, and a bound
    /// table with a stale literal beside it would pass any test that only looked
    /// at the collection.
    /// </summary>
    [Fact]
    public void NoInventedDecodeIsLeftInTheMarkup()
    {
        var markup = DecodedPanelMarkup();

        foreach (var message in Invented)
        {
            var literal = $"Text=\"{message}\"";

            _output.WriteLine($"  {literal,-40} {(markup.Contains(literal, StringComparison.Ordinal) ? "STILL THERE" : "gone")}");

            Assert.DoesNotContain(literal, markup, StringComparison.Ordinal);
        }

        Assert.Contains("{Binding DigitalDecodes}", markup, StringComparison.Ordinal);
    }

    /// <summary>
    /// **NO PANEL ON THE TAB STILL CLAIMS A STATION WAS HEARD.** The decoded
    /// table became real in this unit, so a mode strip counting nine messages or
    /// a plain-English card describing a contact that never happened is not a
    /// placeholder any more — it is the tab disagreeing with itself.
    /// </summary>
    [Fact]
    public void NoPanelOnTheTabStillClaimsAStationWasHeard()
    {
        var markup = DigitalWorkspaceMarkup();

        var claims = new[]
        {
            "reading it &#183; 9 messages this slot",
            "4 stations &#183; 2 contacts running",
            "14:22:45 UTC &#183; 4 shown",
            "W9XYZ is answering K1ABC",
            "K1ABC is calling anyone",
            "EA3QQ is calling for distant stations",
        };

        foreach (var claim in claims)
        {
            var literal = $"\"{claim}\"";

            _output.WriteLine(
                $"  {claim,-42} "
                + $"{(markup.Contains(literal, StringComparison.Ordinal) ? "STILL THERE" : "gone")}");

            Assert.DoesNotContain(literal, markup, StringComparison.Ordinal);
        }

        // **AND THE PANEL THAT LOST ITS CARDS CARRIES ITS OWN IDLE LINE**, rather
        // than being left blank, which is indistinguishable from broken.
        Assert.Contains("{Binding DigitalSayingIdle}", markup, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE PLAIN-ENGLISH PANEL SAYS NOTHING THIS UNIT WROTE** (§12.1). What
    /// Hamlet says a message means is Tim's, so the panel carries the line he
    /// wrote in August and nothing else.
    /// </summary>
    [Fact]
    public void ThePlainEnglishPanelCarriesOnlyTimsOwnIdleLine()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        _output.WriteLine($"  saying [{model.DigitalSayingIdle}]");
        _output.WriteLine($"  strip  [{model.DigitalModeStripLine}]");

        Assert.Equal(DigitalIdleText.Saying, model.DigitalSayingIdle);
        Assert.Equal(DigitalIdleText.ModeStrip, model.DigitalModeStripLine);
    }

    /// <summary>
    /// **THE MODE STRIP REPORTS THE PRESS RATHER THAN A PLACEHOLDER.** After a
    /// decode it carries what the press made of the audio.
    /// </summary>
    [Fact]
    public void TheModeStripReportsWhatThePressFound()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            Recording(48000),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            Measured);

        _output.WriteLine($"  strip [{model.DigitalModeStripLine}]");

        Assert.Equal("one message out of one slot", model.DigitalModeStripLine);
    }

    /// <summary>The whole Digital workspace's markup.</summary>
    private static string DigitalWorkspaceMarkup()
    {
        var markup = File.ReadAllText(
            Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var from = markup.IndexOf(
            "x:Name=\"DigitalWorkspace\"", StringComparison.Ordinal);
        var to = markup.IndexOf(
            "x:Name=\"VoiceWorkspace\"", StringComparison.Ordinal);

        Assert.InRange(from, 0, int.MaxValue);
        Assert.InRange(to, from, int.MaxValue);

        return markup[from..to];
    }

    /// <summary>The decoded panel's own markup, and nothing else's.</summary>
    /// <remarks>
    /// **SCOPED TO ONE PANEL DELIBERATELY.** The plain-English panel below it
    /// still carries the placeholder cards work instruction 037 wrote, and what
    /// Hamlet says a message *means* is Tim's under §12.1 — a test asserting over
    /// the whole file would be this unit ruling on his words by the back door.
    /// </remarks>
    private static string DecodedPanelMarkup()
    {
        var markup = File.ReadAllText(
            Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var from = markup.IndexOf(
            "x:Name=\"DigitalDecodedPanel\"", StringComparison.Ordinal);
        var to = markup.IndexOf(
            "x:Name=\"DigitalSayingPanel\"", StringComparison.Ordinal);

        Assert.InRange(from, 0, int.MaxValue);
        Assert.InRange(to, from, int.MaxValue);

        return markup[from..to];
    }

    /// <summary>Thirty seconds of audio with one transmission in the whole slot.</summary>
    /// <param name="rate">The rate to build it at.</param>
    private static MonoAudio Recording(int rate)
    {
        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(message), rate, 1240f);

        var samples = new float[rate * 30];

        // 14:22:30 is thirteen seconds after a recording ending at 14:22:47 began.
        slot.CopyTo(samples.AsSpan(13 * rate));

        return new MonoAudio(rate, samples);
    }

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
